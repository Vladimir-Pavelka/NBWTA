namespace NBWTA.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using Utils;

    internal static class GridGraph
    {
        private static readonly Node OutOfBoundaryNode = new Node(-1, -1, false);

        public static IReadOnlyCollection<Node> Convert(bool[,] shape, NeighborsMode neighborsMode)
        {
            var widthRange = Enumerable.Range(0, shape.GetLength(0));
            var heightRange = Enumerable.Range(0, shape.GetLength(1));

            var nodes = widthRange.Select(column =>
                heightRange.Select(row =>
                        new Node(column, row, shape[column, row]))
                    .ToList()).ToList();

            SetNeighbors(nodes, neighborsMode);

            return nodes.SelectMany(x => x).ToList();
        }

        private static void SetNeighbors(List<List<Node>> allNodes, NeighborsMode neighborsMode)
        {
            var isFourNeighborsMode = neighborsMode == NeighborsMode.FourNeighbors;
            allNodes.SelectMany(x => x)
                .ForEach(node =>
                {
                    var nodeNeighbors = GetNeighbors(node, allNodes, isFourNeighborsMode).ToList();
                    node.Neighbors.AddRange(nodeNeighbors);
                });
        }

        private static IEnumerable<Node> GetNeighbors(Node node, List<List<Node>> allNodes, bool onlyStraight)
        {
            var width = allNodes.Count;
            var height = allNodes[0].Count;

            var left = node.X - 1;
            var right = node.X + 1;
            var top = node.Y - 1;
            var bottom = node.Y + 1;

            var hasTop = top >= 0;
            var hasBottom = bottom < height;
            var hasLeft = left >= 0;
            var hasRight = right < width;

            yield return hasTop ? allNodes[node.X][top] : OutOfBoundaryNode;
            if (!onlyStraight) yield return hasRight && hasTop ? allNodes[right][top] : OutOfBoundaryNode;
            yield return hasRight ? allNodes[right][node.Y] : OutOfBoundaryNode;
            if (!onlyStraight) yield return hasRight && hasBottom ? allNodes[right][bottom] : OutOfBoundaryNode;
            yield return hasBottom ? allNodes[node.X][bottom] : OutOfBoundaryNode;
            if (!onlyStraight) yield return hasLeft && hasBottom ? allNodes[left][bottom] : OutOfBoundaryNode;
            yield return hasLeft ? allNodes[left][node.Y] : OutOfBoundaryNode;
            if (!onlyStraight) yield return hasLeft && hasTop ? allNodes[left][top] : OutOfBoundaryNode;
        }
    }

    public enum NeighborsMode
    {
        FourNeighbors,
        EightNeighbors
    }
}