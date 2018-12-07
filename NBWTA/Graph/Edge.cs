namespace NBWTA.Graph
{
    using System.Collections.Generic;
    using System.Linq;

    public class Edge
    {
        public IReadOnlyCollection<Node> Path { get; }
        public Vertex ConnectedVertex { get; }

        public Edge(IReadOnlyCollection<Node> path, Vertex connectedVertex)
        {
            Path = path;
            ConnectedVertex = connectedVertex;
        }

        public override int GetHashCode() => Path.Any() ? Path.First().GetHashCode() ^ Path.Last().GetHashCode() : 0;

        public override bool Equals(object other) => other is Edge otherEdge && Equals(otherEdge);

        public bool Equals(Edge other)
        {
            if (Path.Any() && other.Path.Any())
            {
                return !new[] { Path.First(), Path.Last() }.Except(new[] { other.Path.First(), other.Path.Last() }).Any();
            }

            return ReferenceEquals(this, other);
        }
    }
}