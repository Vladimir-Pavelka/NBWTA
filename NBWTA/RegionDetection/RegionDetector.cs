namespace NBWTA.RegionDetection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Graph;
    using Utils;

    internal static class RegionDetector
    {
        public static IEnumerable<Region<Node>> FindRegions(IReadOnlyCollection<Node> mapNodes, Node[,] nodesMap, Func<int, int, bool> isValid)
        {
            var width = nodesMap.GetLength(0);
            var height = nodesMap.GetLength(1);

            var processed = new HashSet<Node>();

            foreach (var node in mapNodes)
            {
                if (processed.Contains(node) || !isValid(node.X, node.Y)) continue;
                var regionXys = FloodFill.Scanline(node.X, node.Y, width, height, isValid);
                var regionNodes = regionXys.Select(xy => nodesMap[xy.x, xy.y]).ToHashSet();
                processed.UnionWith(regionNodes);
                yield return new Region<Node>(regionNodes);
            }
        }
    }
}