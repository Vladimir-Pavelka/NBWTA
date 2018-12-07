namespace Analyzer.DistanceTransformation
{
    using System.Collections.Generic;
    using System.Linq;
    using Utils;

    public static class DistanceTransform
    {
        private static readonly Clearance _outOfBoundaryClearance = new Clearance();

        public static Clearance[,] Process(bool[,] shape)
        {
            var result = new Clearance[shape.GetLength(0), shape.GetLength(1)];

            shape.ForEach((x, y, isClear) =>
            {
                var clearance = new Clearance();
                result[x, y] = clearance;

                if (isClear)
                {
                    var neighbors = GetNeighbors(x, y, result).ToList();
                    clearance.Top = neighbors[0].Top + 1;
                    clearance.TopLeft = neighbors[1].TopLeft + 1;
                    clearance.Left = neighbors[2].Left + 1;
                    clearance.BotLeft = neighbors[3].BotLeft + 1;
                    return;
                }

                clearance.Top = 0;
                clearance.TopLeft = 0;
                clearance.Left = 0;
                clearance.BotLeft = 0;
            });

            shape.ForEachReverse((x, y, isClear) =>
            {
                var clearance = result[x, y];

                if (isClear)
                {
                    var neighbors = GetNeighborsReverse(x, y, result).ToList();
                    clearance.TopRight = neighbors[0].TopRight + 1;
                    clearance.Right = neighbors[1].Right + 1;
                    clearance.BotRight = neighbors[2].BotRight + 1;
                    clearance.Bot = neighbors[3].Bot + 1;
                    return;
                }

                clearance.TopRight = 0;
                clearance.Right = 0;
                clearance.BotRight = 0;
                clearance.Bot = 0;
            });

            return result;
        }

        private static IEnumerable<Clearance> GetNeighbors(int x, int y, Clearance[,] grid)
        {
            var height = grid.GetLength(1);

            var top = y - 1;
            var left = x - 1;
            var bot = y + 1;

            var hasTop = top >= 0;
            var hasLeft = left >= 0;
            var hasBot = bot < height;

            yield return hasTop ? grid[x, top] : _outOfBoundaryClearance;
            yield return hasLeft && hasTop ? grid[left, top] : _outOfBoundaryClearance;
            yield return hasLeft ? grid[left, y] : _outOfBoundaryClearance;
            yield return hasLeft && hasBot ? grid[left, bot] : _outOfBoundaryClearance;
        }

        private static IEnumerable<Clearance> GetNeighborsReverse(int x, int y, Clearance[,] grid)
        {
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);

            var top = y - 1;
            var right = x + 1;
            var bot = y + 1;

            var hasTop = top >= 0;
            var hasRight = right < width;
            var hasBot = bot < height;

            yield return hasRight && hasTop ? grid[right, top] : _outOfBoundaryClearance;
            yield return hasRight ? grid[right, y] : _outOfBoundaryClearance;
            yield return hasRight && hasBot ? grid[right, bot] : _outOfBoundaryClearance;
            yield return hasBot ? grid[x, bot] : _outOfBoundaryClearance;
        }
    }
}