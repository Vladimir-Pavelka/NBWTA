namespace NBWTA.MapPreprocessing
{
    using System.Collections.Generic;
    using System.Linq;
    using RegionDetection;
    using Utils;

    internal static class Preprocessing
    {
        public static bool[,] RemoveTinyIsolatedObstacles(bool[,] shape)
        {
            var obstacleRegions = FindObstacleRegions(shape);
            var result = new bool[shape.GetLength(0), shape.GetLength(1)]; // TODO: analyze walkables as well, identify islands, drop holes
            result.ForEach((x, y, _) => result[x, y] = true);
            obstacleRegions.Where(IsBigEnough).SelectMany(r => r.Nodes).ForEach(node => result[node.X, node.Y] = false);
            return result;
        }

        private static IEnumerable<Region<(int X, int Y)>> FindObstacleRegions(bool[,] shape)
        {
            var width = shape.GetLength(0);
            var height = shape.GetLength(1);

            var processed = new HashSet<(int, int)>();
            bool IsWall(int x, int y) => !shape[x, y];

            var iterationIndices = Enumerable.Range(0, width).SelectMany(x => Enumerable.Range(0, height).Select(y => (X: x, Y: y)));
            foreach (var coord in iterationIndices)
            {
                if (processed.Contains(coord) || !IsWall(coord.X, coord.Y)) continue;
                var regionCoords = FloodFill.Scanline(coord.X, coord.Y, width, height, IsWall);
                var regionCoordsHashSet = regionCoords.ToHashSet();
                processed.UnionWith(regionCoordsHashSet);
                yield return new Region<(int, int)>(regionCoordsHashSet);
            }
        }

        private static bool IsBigEnough(Region<(int, int)> region)
        {
            var boundingBox = Extensions.BoundingBox(region.Nodes);
            var width = boundingBox.botRight.x - boundingBox.topLeft.x;
            var height = boundingBox.botRight.y - boundingBox.topLeft.y;

            return width > 15 || height > 15;
        }
    }
}