namespace NBWTA.Graph
{
    using System.Collections.Generic;
    using System.Drawing;

    internal class Node
    {
        public Node(int x, int y, bool belongsToShape)
        {
            X = x;
            Y = y;
            BelongsToShape = belongsToShape;
        }

        public int X { get; }
        public int Y { get; }
        public bool BelongsToShape { get; set; }
        public List<Node> Neighbors { get; set; } = new List<Node>();

        public bool IsLeaf => Neighbors.Count <= 1;
        public bool IsEdge => Neighbors.Count == 2;
        public bool IsVertex => Neighbors.Count >= 3;

        public Point ToPoint() => new Point(X, Y);
    }
}