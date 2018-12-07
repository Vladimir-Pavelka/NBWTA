namespace NBWTA.Skeletonization
{
    using System.Collections.Generic;
    using System.Linq;
    using Graph;
    using Utils;

    public static class HilditchThinning
    {
        public static IEnumerable<Node> Skeletonize(IEnumerable<Node> graph)
        {
            var remainingNodes = graph.Where(node => node.BelongsToShape).ToList();

            while (true)
            {
                var groupedByIsMarkedForDeletion = remainingNodes.GroupBy(ShouldBeDeleted).ToList();
                var nodesMarkedForDeletion = groupedByIsMarkedForDeletion.FirstOrDefault(IsMarkedForDeletion);
                if (nodesMarkedForDeletion == null) return remainingNodes;

                nodesMarkedForDeletion.ForEach(node => node.BelongsToShape = false);
                remainingNodes = groupedByIsMarkedForDeletion.First(group => !IsMarkedForDeletion(group)).ToList();
            }
        }

        private static bool IsMarkedForDeletion(IGrouping<bool, Node> group) => group.Key;

        private static bool ShouldBeDeleted(Node node)
        {
            const int neighborIndexP2 = 0;
            const int neighborIndexP4 = 2;
            const int neighborIndexP6 = 4;
            const int neighborIndexP8 = 6;

            var numberOfNonZeroNeighbors = node.Neighbors.Count(neighbor => neighbor.BelongsToShape);

            var condition1 = 2 <= numberOfNonZeroNeighbors && numberOfNonZeroNeighbors <= 6;

            bool Condition2() => NeighborZeroOneTransitionsCount(node) == 1;

            bool Condition3() => !node.Neighbors[neighborIndexP2].BelongsToShape ||
                                 !node.Neighbors[neighborIndexP4].BelongsToShape ||
                                 !node.Neighbors[neighborIndexP8].BelongsToShape ||
                                 NeighborZeroOneTransitionsCount(node.Neighbors[neighborIndexP2]) != 1;

            bool Condition4() => !node.Neighbors[neighborIndexP2].BelongsToShape ||
                                 !node.Neighbors[neighborIndexP4].BelongsToShape ||
                                 !node.Neighbors[neighborIndexP6].BelongsToShape ||
                                 NeighborZeroOneTransitionsCount(node.Neighbors[neighborIndexP4]) != 1;

            return condition1 && Condition2() && Condition3() && Condition4();
        }

        private static int NeighborZeroOneTransitionsCount(Node node)
        {
            var nodeNeighbors = node.Neighbors;
            if (!nodeNeighbors.Any()) return 0;

            var shiftedByOne = nodeNeighbors.Skip(1).Concat(nodeNeighbors.First().Yield());

            return nodeNeighbors
                .Zip(shiftedByOne, IsZeroToOneTransition)
                .Sum(isTransition => isTransition ? 1 : 0);
        }

        private static bool IsZeroToOneTransition(Node previous, Node next) => !previous.BelongsToShape && next.BelongsToShape;
    }
}