namespace NBWTA.Result
{
    using System.Collections.Generic;
    using System.Linq;
    using ChokePointsDetection;
    using Graph;
    using RegionDetection;
    using Utils;

    internal static class Results
    {
        public static AnalyzedMap Create(IReadOnlyCollection<Choke> chokes, IEnumerable<Region<Node>> nonChokeRegions,
            IDictionary<ChokeBorder, (Region<Node>[] left, Region<Node>[] right)> chokeRegionMap,
            HashSet<Node> allChokeNodes, IDictionary<Region<Node>, HashSet<ChokeBorder>> regionChokeMap,
            Dictionary<ChokeBorder, Choke> borderChokeMap)
        {
            var chokeMap = chokes.ToDictionary(ch => ch, CreateChokeRegion);
            var regionMap = nonChokeRegions.ToDictionary(r => r, CreateMapRegion);
            chokeMap.ForEach(kvp => kvp.Value.AdjacentRegions = GetAdjacentRegions(kvp.Key, chokeRegionMap, allChokeNodes, regionMap));
            regionMap.ForEach(kvp => kvp.Value.AdjacentChokes = GetAdjacentChokes(kvp.Key, regionChokeMap, borderChokeMap, chokeMap));

            return new AnalyzedMap(regionMap.Values, chokeMap.Values);
        }

        private static ChokeRegion CreateChokeRegion(Choke choke)
        {
            var contentTiles = choke.Fill.Select(n => (n.X, n.Y)).ToHashSet();
            var regionEdge = choke.Border.StartBorder.WholeLine.Concat(choke.Border.EndBorder.WholeLine)
                .Select(n => (n.X, n.Y)).ToList();
            return new ChokeRegion(contentTiles, regionEdge);
        }

        private static MapRegion CreateMapRegion(Region<Node> region)
        {
            var contentTiles = region.Nodes.Select(n => (n.X, n.Y)).ToHashSet();
            var regionEdge = GetRegionEdge(region).Select(n => (n.X, n.Y)).ToList();
            return new MapRegion(contentTiles, regionEdge);
        }

        private static IEnumerable<Node> GetRegionEdge(Region<Node> region) => region.Nodes.Where(n => n.Neighbors.Any(nn => !nn.BelongsToShape));

        // TODO: find a more elegant way :/
        private static IReadOnlyCollection<MapRegion> GetAdjacentRegions(Choke choke,
            IDictionary<ChokeBorder, (Region<Node>[] left, Region<Node>[] right)> chokeRegionMap,
            HashSet<Node> allChokeNodes,
            IDictionary<Region<Node>, MapRegion> regionMap)
        {
            var adjacentRegions = chokeRegionMap[choke.Border].left.Concat(chokeRegionMap[choke.Border].right)
                .Where(r => !allChokeNodes.Contains(r.Nodes.First())).Select(r => regionMap[r])
                .ToList();

            return adjacentRegions;
        }

        private static IReadOnlyCollection<ChokeRegion> GetAdjacentChokes(Region<Node> region,
            IDictionary<Region<Node>, HashSet<ChokeBorder>> regionChokeMap,
            IDictionary<ChokeBorder, Choke> borderChokeMap,
            IDictionary<Choke, ChokeRegion> chokeMap)
        {
            if (!regionChokeMap.ContainsKey(region)) return new List<ChokeRegion>();
            var adjacentChokes = regionChokeMap[region].Select(chb => borderChokeMap[chb])
                .Select(ch => chokeMap[ch]).ToList();

            return adjacentChokes;
        }
    }
}