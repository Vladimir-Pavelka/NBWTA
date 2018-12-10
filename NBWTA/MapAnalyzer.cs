namespace NBWTA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ChokePointsDetection;
    using DistanceTransformation;
    using Graph;
    using Mappings;
    using MapPreprocessing;
    using MedialAxisPruning;
    using RegionDetection;
    using Result;
    using Skeletonization;
    using Utils;

    public class MapAnalyzer : IMapAnalyzer
    {
        private const int ShrinkMinClearance = 3;
        private const int PruningMinClearance = 15;

        public AnalyzedMap Analyze(int width, int height, Func<(int x, int y), bool> isWalkable)
        {
            var map = Create2DBoolMap(width, height, isWalkable);
            var preprocessedMap = Preprocessing.RemoveTinyIsolatedObstacles(map);
            var clearance = DistanceTransform.Process(preprocessedMap);
            var graphEightNeighbors = GridGraph.Convert(preprocessedMap, NeighborsMode.EightNeighbors); // TODO: we dont even really need the mode
            ShrinkGraphToAccelerateSkeletonization(graphEightNeighbors, clearance, ShrinkMinClearance); // TODO: optional.. potentially harmful
            var skeleton = HilditchThinning.Skeletonize(graphEightNeighbors).ToList();
            var connectedVertexEdgeGraph = ConnectAsVertexEdgeGraph(skeleton);
            var prunedSkeleton = Pruning.Prune(connectedVertexEdgeGraph, clearance, PruningMinClearance);

            var graphFourNeighbors = GridGraph.Convert(preprocessedMap, NeighborsMode.FourNeighbors); // TODO: we dont even really need the mode
            var chokeBorders = ChokePointsDetector.GetChokeBorders(prunedSkeleton, tile => IsWallOrOutOfBounds(tile, preprocessedMap));
            var tileIsChokeMap = Mapping.CreateTileIsChokeMap(width, height, chokeBorders);
            SetChokeTilesAsWall(graphFourNeighbors, tileIsChokeMap);
            var tileNodeMap = Mapping.CreateTileNodeMap(width, height, graphFourNeighbors);

            var allRegions = RegionDetector.FindRegions(graphFourNeighbors, tileNodeMap, (x, y) => tileNodeMap[x, y].BelongsToShape).ToList();
            var pointRegionMap = Mapping.CreateNodeRegionMap(width, height, allRegions);
            var chokeRegionMap = Mapping.CreateChokeRegionMap(chokeBorders, pointRegionMap);
            var regionChokeMap = Mapping.CreateRegionChokeMap(chokeRegionMap);

            var chokes = ChokePointsDetector.CreateFilledChokeRegions(chokeBorders, chokeRegionMap, regionChokeMap, tileNodeMap);
            var allChokeNodes = chokes.SelectMany(ch => ch.Fill).ToHashSet();
            var nonChokeRegions = allRegions.Where(r => !allChokeNodes.Contains(r.Nodes.First()));

            var chokeRegions = CreateChokeRegions(chokes);
            var mapRegions = CreateMapRegions(nonChokeRegions);

            return new AnalyzedMap(mapRegions, chokeRegions);
        }

        private static bool[,] Create2DBoolMap(int width, int height, Func<(int x, int y), bool> isWalkable)
        {
            var map = new bool[width, height];
            map.ForEach((x, y, _) => map[x, y] = isWalkable((x, y)));
            return map;
        }

        private static void ShrinkGraphToAccelerateSkeletonization(IEnumerable<Node> graph, Clearance[,] distanceTransform, int minClearance)
        {
            graph.Where(node => IsCloserToWallThan(node, distanceTransform, minClearance))
                .ForEach(node => node.BelongsToShape = false);
        }

        private static bool IsCloserToWallThan(Node n, Clearance[,] clearance, int minClearance) =>
            clearance[n.X, n.Y].AllDirections.Min() <= minClearance;

        private static IReadOnlyCollection<Vertex> ConnectAsVertexEdgeGraph(IReadOnlyCollection<Node> skeleton)
        {
            VertexEdgeGraph.SetConnectingNeighbors(skeleton);
            var connectedGraph = VertexEdgeGraph.ConnectVertices(skeleton).ToList();
            return connectedGraph;
        }

        public static bool IsWallOrOutOfBounds((int X, int Y) tile, bool[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            if (tile.X < 0 || tile.X >= width) return true;
            if (tile.Y < 0 || tile.Y >= height) return true;

            return !map[tile.X, tile.Y];
        }

        private static void SetChokeTilesAsWall(IEnumerable<Node> nodes, bool[,] tileIsChokeMap) =>
            nodes.Where(n => tileIsChokeMap[n.X, n.Y]).ForEach(n => n.BelongsToShape = false);

        private static IReadOnlyCollection<ChokeRegion> CreateChokeRegions(IEnumerable<Choke> chokes) =>
            chokes.Select(ch =>
            {
                var contentTiles = ch.Fill.Select(n => (n.X, n.Y)).ToHashSet();
                var regionEdge = ch.Border.StartBorder.WholeLine.Concat(ch.Border.EndBorder.WholeLine)
                    .Select(n => (n.X, n.Y)).ToList();
                return new ChokeRegion(contentTiles, regionEdge);
            }).ToList();

        private static IReadOnlyCollection<MapRegion> CreateMapRegions(IEnumerable<Region<Node>> nonChokeRegions) =>
            nonChokeRegions.Select(r =>
            {
                var contentTiles = r.Nodes.Select(n => (n.X, n.Y)).ToHashSet();
                var regionEdge = GetRegionEdge(r).Select(n => (n.X, n.Y)).ToList();
                return new MapRegion(contentTiles, regionEdge);
            }).ToList();

        private static IEnumerable<Node> GetRegionEdge(Region<Node> region) => region.Nodes.Where(n => n.Neighbors.Any(nn => !nn.BelongsToShape));
    }
}