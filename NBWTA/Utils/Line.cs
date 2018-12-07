namespace NBWTA.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class Line
    {
        public double Slope { get; }
        public double Offset { get; }

        private Line(double slope, double offset)
        {
            Slope = slope;
            Offset = offset;
        }

        public static Line FromPoints(Point a, Point b)
        {
            var isVertical = a.X == b.X;
            if (isVertical) return new Line(double.MaxValue, a.X);

            var slope = (b.Y - a.Y) / (b.X - a.X);
            var offset = a.Y - slope * a.X;
            return new Line(slope, offset);
        }

        public static Line FromPoints(Point[] points)
        {
            var xAvg = points.Average(p => p.X);
            var yAvg = points.Average(p => p.Y);

            var xDiffSquaredSum = points.Sum(p => (p.X - xAvg) * (p.X - xAvg));
            var isVertical = AreEqualApproximately(xDiffSquaredSum, 0);
            if (isVertical) return new Line(double.MaxValue, xAvg);

            var slope = points.Sum(p => (p.X - xAvg) * (p.Y - yAvg)) / xDiffSquaredSum;
            var offset = yAvg - slope * xAvg;
            return new Line(slope, offset);
        }

        public bool ContainsPoint((double x, double y) point) => IsVertical
            ? AreEqualApproximately(point.x, Offset)
            : AreEqualApproximately(Slope * point.x + Offset, point.y);

        public Line GetPerpendicularAt(Point point)
        {
            //GuardContainsPoint(point);  There is a perpendicular line for any point - doesn't require to lie on the original
            if (IsHorizontal) return new Line(double.MaxValue, point.X);
            if (IsVertical) return new Line(0, point.Y);

            var newSlope = -1 / Slope;
            var newOffset = point.Y - newSlope * point.X;
            return new Line(newSlope, newOffset);
        }

        public (double x, double y) GetClosestLinePointTo(Point point)
        {
            var perpendicular = GetPerpendicularAt(point);
            var intersection = GetPerpendicularIntersection(perpendicular);
            return intersection;
        }

        private (double x, double y) GetPerpendicularIntersection(Line perpendicular)
        {
            if (IsVertical) return (Offset, perpendicular.Offset);
            if (IsHorizontal) return (perpendicular.Offset, Offset);

            var intersectionX = (perpendicular.Offset - Offset) / (Slope - perpendicular.Slope);
            var intersectionY = GetYAt(intersectionX);
            return (intersectionX, intersectionY);
        }

        public double GetYAt(double x) => IsVertical
            ? throw new InvalidOperationException("Vertical line, ambiguous Y for given X")
            : Slope * x + Offset;

        public double GetXAt(double y) => IsHorizontal
            ? throw new InvalidOperationException("Horizontal line, ambiguous X for given Y")
            : (y - Offset) / Slope;

        public bool IsVertical => AreEqualApproximately(Slope, double.MaxValue);
        public bool IsHorizontal => AreEqualApproximately(Slope, 0);

        public IEnumerable<Point> LeftIndexesFrom((double x, double y) point) => IsVertical
            ? VerticalIndexesFrom(point, offset => point.y - offset)
            : IndexesFrom(point, offset => point.x - offset);

        public IEnumerable<Point> RightIndexesFrom((double x, double y) point) => IsVertical
            ? VerticalIndexesFrom(point, offset => point.y + offset)
            : IndexesFrom(point, offset => point.x + offset);

        private IEnumerable<Point> IndexesFrom((double x, double y) point, Func<double, double> offsetOperation)
        {
            GuardContainsPoint(point);
            var slopeSqrt = Math.Sqrt(1 + Slope * Slope); // Point along the line in distance D: x = x0 +- D/Sqrt(1 + Slope^2)
            return Enumerable.Range(1, int.MaxValue - 1).Select(d => d / slopeSqrt).Select(offsetOperation)
                .Select(newX => new Point(Round(newX), Round(GetYAt(newX))));
        }

        private static int Round(double x) => (int)Math.Round(x);

        private IEnumerable<Point> VerticalIndexesFrom((double x, double y) point, Func<int, double> offsetOperation)
        {
            GuardContainsPoint(point);
            return Enumerable.Range(1, int.MaxValue - 1).Select(offsetOperation)
                .Select(newY => new Point((int)point.x, (int)newY));
        }

        private void GuardContainsPoint((double x, double y) point)
        {
            if (!ContainsPoint(point))
                throw new InvalidOperationException("Given point doesn't lie on the line");
        }

        public static bool AreEqualApproximately(double a, double b)
        {
            const double epsilon = 0.00000000001;
            return Math.Abs(a - b) < epsilon;
        }
    }
}