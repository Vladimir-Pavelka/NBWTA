namespace NBWTA.MedialAxisPruning
{
    using System.Collections.Generic;
    using System.Linq;
    using DistanceTransformation;
    using Graph;
    using Utils;

    internal static class Pruning
    {
        public static IReadOnlyCollection<Vertex> Prune(IReadOnlyCollection<Vertex> skeleton, Clearance[,] clearance, int pruneDistance)
        {
            var remaining = skeleton.ToList();
            while (true)
            {
                var leavesToPrune = GetLeavesToPrune(remaining, clearance, pruneDistance);
                if (!leavesToPrune.Any()) break;

                leavesToPrune.ForEach(x =>
                {
                    RemoveSelfFromConnectedVertices(x);
                    RemoveSelfFromShape(x);
                });

                remaining = remaining.Except(leavesToPrune).ToList();
            }

            var prunedSkeleton = RepairEdgesAfterRemovingVertices(remaining);
            return prunedSkeleton;
        }

        private static IReadOnlyCollection<Vertex> GetLeavesToPrune(IEnumerable<Vertex> skeleton, Clearance[,] clearance, int pruneDistance)
            => skeleton.Where(vertex => vertex.Node.IsLeaf)
                .Where(vertex => DistanceToClosesObstacle(vertex.Node, clearance) <= pruneDistance)
                .ToList();

        private static void RemoveSelfFromConnectedVertices(Vertex vertexToRemove) =>
            vertexToRemove.Edges.ForEach(sourceConn =>
            {
                sourceConn.ConnectedVertex.RemoveEdges(destConn => destConn.ConnectedVertex == vertexToRemove);
                sourceConn.ConnectedVertex.Node.Neighbors.Remove(sourceConn.Path.Any() ? sourceConn.Path.Last() : vertexToRemove.Node);
            });

        private static int DistanceToClosesObstacle(Node node, Clearance[,] clearance) =>
            clearance[node.X, node.Y].AllDirections.Min();

        private static void RemoveSelfFromShape(Vertex vertexToRemove)
        {
            vertexToRemove.Node.BelongsToShape = false;
            vertexToRemove.Edges.ForEach(conn => conn.Path.ForEach(n => n.BelongsToShape = false));
        }

        private static IReadOnlyCollection<Vertex> RepairEdgesAfterRemovingVertices(IReadOnlyCollection<Vertex> connectedGraph)
        {
            var allNodes = connectedGraph.SelectMany(v => v.Edges.SelectMany(c => c.Path)).Distinct()
                .Concat(connectedGraph.Select(v => v.Node));

            return VertexEdgeGraph.ConnectVertices(allNodes).ToList();
        }
    }
}