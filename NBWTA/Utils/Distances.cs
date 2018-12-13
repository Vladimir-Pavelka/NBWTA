namespace NBWTA.Utils
{
    using System;

    public static class Distances
    {
        public static double Euclidean((int x, int y) left, (int x, int y) right)
        {
            var xDistance = left.x - right.x;
            var yDistance = left.y - right.y;

            return Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }

        public static double EuclideanSquared((int x, int y) left, (int x, int y) right)
        {
            var xDistance = left.x - right.x;
            var yDistance = left.y - right.y;

            return xDistance * xDistance + yDistance * yDistance;
        }

        public static double Diagonal((int x, int y) left, (int x, int y) right)
        {
            const int straightCost = 10;
            const int diagonalCost = 14;

            var xDistance = Math.Abs(left.x - right.x);
            var yDistance = Math.Abs(left.y - right.y);

            return xDistance > yDistance
                ? straightCost * (xDistance - yDistance) + diagonalCost * yDistance
                : straightCost * (yDistance - xDistance) + diagonalCost * xDistance;
        }

        public static double Manhattan((int x, int y) left, (int x, int y) right)
        {
            var xDistance = Math.Abs(left.x - right.x);
            var yDistance = Math.Abs(left.y - right.y);

            return xDistance + yDistance;
        }
    }
}