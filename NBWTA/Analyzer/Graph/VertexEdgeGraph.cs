namespace Analyzer.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using Utils;

    public static class VertexEdgeGraph
    {
        public static void SetConnectingNeighbors(IEnumerable<Node> skeleton) =>
            skeleton.ForEach(node => node.Neighbors = GetConnectingNeighbors(node).ToList());

        private static IEnumerable<Node> GetConnectingNeighbors(Node node)
        {
            var bannedNeighbors = GetBannedNeighbors(node);
            return node.Neighbors.Where(n => n.BelongsToShape).Where(n => !bannedNeighbors.Contains(n));
        }

        private static HashSet<Node> GetBannedNeighbors(Node node)
        {
            var bannedNeighbors = Enumerable.Range(0, 8) // top, topRight, right, botRight, bot, botLeft, left, topLeft
                .Where(directon => directon % 2 == 0) // top, right, bot, left
                .Where(direction => node.Neighbors[direction].BelongsToShape)
                .SelectMany(direction => new[] { direction - 1, direction + 1 }) // if belongs to skeleton, its diagonal neighbors are banned
                .Select(bannedDirection => bannedDirection == -1 ? 7 : bannedDirection) // top = 0; -1 -> 7
                .Select(bannedDirection => node.Neighbors[bannedDirection]);

            return new HashSet<Node>(bannedNeighbors);
        }

        public static IEnumerable<Vertex> ConnectVertices(IEnumerable<Node> skeleton)
        {
            var grouped = skeleton.GroupBy(node => node.Neighbors.Count).ToList();
            var leaves = grouped.First(g => g.Key == 1).ToList();
            var vertices = grouped.Where(g => g.Key >= 3).SelectMany(x => x).ToList();

            var nodeVertexMap = leaves.Concat(vertices).ToDictionary(n => n, n => new Vertex(n));
            var allVertices = nodeVertexMap.Values.ToList();

            allVertices.ForEach(vertex => vertex.Edges =
                vertex.Node.Neighbors.Select(nn => PathToNextVertex(nn, vertex.Node))
                    .Select(path => new Edge(path.path.ToList(), nodeVertexMap[path.to])).ToList());

            return allVertices;
        }

        private static (IEnumerable<Node> path, Node to) PathToNextVertex(Node start, Node parent)
        {
            var path = new List<Node>();
            var previous = parent;
            var current = start;
            while (current.IsEdge)
            {
                path.Add(current);
                var next = current.Neighbors.First(n => n != previous);
                previous = current;
                current = next;
            }

            var destinationVertex = current;
            return (path, destinationVertex);
        }
    }
}