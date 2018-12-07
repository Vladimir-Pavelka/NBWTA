namespace Analyzer.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Vertex
    {
        public Node Node { get; }
        public IReadOnlyCollection<Edge> Edges { get; set; } = new List<Edge>();

        public Vertex(Node node)
        {
            Node = node;
        }

        public void RemoveEdges(Func<Edge, bool> predicate)
        {
            var edgesToRemove = Edges.Where(predicate).ToList();
            var nodeNeighborsToRemove = edgesToRemove.Select(c => c.Path.FirstOrDefault() ?? c.ConnectedVertex.Node);
            Node.Neighbors = Node.Neighbors.Except(nodeNeighborsToRemove).ToList();
            Edges = Edges.Except(edgesToRemove).ToList();
        }
    }
}