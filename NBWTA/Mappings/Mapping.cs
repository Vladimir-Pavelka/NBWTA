namespace NBWTA.Mappings
{
    using System.Collections.Generic;
    using System.Linq;
    using ChokePointsDetection;
    using Graph;
    using RegionDetection;
    using Result;
    using Utils;

    internal static class Mapping
    {
        private const int BuildTileToWalktileRatio = 4;

        public static bool[,] CreateTileIsChokeMap(int width, int height, IEnumerable<ChokeBorder> chokes)
        {
            var chokePointsMap = new bool[width, height];
            chokes.SelectMany(choke => choke.StartBorder.WholeLine.Concat(choke.EndBorder.WholeLine)).ForEach(p => chokePointsMap[p.X, p.Y] = true);
            return chokePointsMap;
        }

        public static Node[,] CreateTileNodeMap(int width, int height, IEnumerable<Node> nodes)
        {
            var nodesMap = new Node[width, height];
            nodes.ForEach(n => nodesMap[n.X, n.Y] = n);
            return nodesMap;
        }

        public static Region<Node>[,] CreateNodeRegionMap(int width, int height, IEnumerable<Region<Node>> regions)
        {
            var nodeRegionMap = new Region<Node>[width, height];
            regions.SelectMany(r => r.Nodes.Select(n => (node: n, region: r))).ForEach(tuple => nodeRegionMap[tuple.node.X, tuple.node.Y] = tuple.region);
            return nodeRegionMap;
        }

        public static IDictionary<(int x, int y), MapRegion> CreateBuildTileRegionMap(IEnumerable<MapRegion> regions)
        {
            var walkTileRegionMap = regions.SelectMany(r => r.ContentTiles.Select(walkTile => (tile: ToBuildTile(walkTile), region: r)))
                .GroupBy(x => x.tile)
                .Select(g => (tile: g.Key, region: g.First().region))
                .ToDictionary(x => x.tile, x => x.region);

            return walkTileRegionMap;
        }

        private static (int x, int y) ToBuildTile((int x, int y) walkTile) => (walkTile.x / BuildTileToWalktileRatio, walkTile.y / BuildTileToWalktileRatio);

        // TODO: improve code
        public static IDictionary<ChokeBorder, (Region<Node>[] left, Region<Node>[] right)> CreateChokeRegionMap(IEnumerable<ChokeBorder> chokes, Region<Node>[,] pointRegionMap)
        {
            var width = pointRegionMap.GetLength(0);
            var height = pointRegionMap.GetLength(1);
            return chokes.ToDictionary(choke => choke, choke =>
            {
                var leftNeighbors = choke.StartBorder.WholeLine.SelectMany(p => YieldNeighborIndices(p.X, p.Y, width, height))
                    .Distinct();
                var leftNeighborRegions = leftNeighbors.Select(n => pointRegionMap[n.x, n.y]).Where(r => r != null)
                    .Distinct().ToArray();

                var rightNeighbors = choke.EndBorder.WholeLine.SelectMany(p => YieldNeighborIndices(p.X, p.Y, width, height))
                    .Distinct();
                var rightNeighborRegions = rightNeighbors.Select(n => pointRegionMap[n.x, n.y]).Where(r => r != null)
                    .Distinct().ToArray();

                return (leftNeighborRegions, rightNeighborRegions);
            });
        }

        private static IEnumerable<(int x, int y)> YieldNeighborIndices(int x, int y, int width, int height)
        {
            var left = x - 1;
            var right = x + 1;
            var top = y - 1;
            var bottom = y + 1;

            var hasTop = top >= 0;
            var hasBottom = bottom < height;
            var hasLeft = left >= 0;
            var hasRight = right < width;

            if (hasTop) yield return (x, top);
            if (hasRight && hasTop) yield return (right, top);
            if (hasRight) yield return (right, y);
            if (hasRight && hasBottom) yield return (right, bottom);
            if (hasBottom) yield return (x, bottom);
            if (hasLeft && hasBottom) yield return (left, bottom);
            if (hasLeft) yield return (left, y);
            if (hasLeft && hasTop) yield return (left, top);
        }

        public static IDictionary<Region<Node>, HashSet<ChokeBorder>> CreateRegionChokeMap(IDictionary<ChokeBorder, (Region<Node>[] left, Region<Node>[] right)> chokeRegionMap)
        {
            var result = new Dictionary<Region<Node>, HashSet<ChokeBorder>>();

            chokeRegionMap.ForEach(kvp =>
            {
                kvp.Value.left.ForEach(r =>
                {
                    if (!result.ContainsKey(r)) result[r] = new HashSet<ChokeBorder>();
                    result[r].Add(kvp.Key);
                });

                kvp.Value.right.ForEach(r =>
                {
                    if (!result.ContainsKey(r)) result[r] = new HashSet<ChokeBorder>();
                    result[r].Add(kvp.Key);
                });
            });

            return result;
        }
    }
}