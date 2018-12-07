namespace NBWTA.DistanceTransformation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Utils;

    public class PerpendicularClearance
    {
        private const int MaxSteps = 60;

        public static PerpendicularClearance Get(IReadOnlyCollection<Point> linePoints, Point at, Func<(int x, int y), bool> isWall)
        {
            var line = Line.FromPoints(linePoints.ToArray());
            var perpendicular = line.GetPerpendicularAt(at);

            var leftClearance = YieldClearancePoints(perpendicular.LeftIndexesFrom((at.X, at.Y)), isWall);
            var rightClearance = YieldClearancePoints(perpendicular.RightIndexesFrom((at.X, at.Y)), isWall);

            return new PerpendicularClearance(at, leftClearance.ToList(), rightClearance.ToList());
        }

        private static IEnumerable<Point> YieldClearancePoints(IEnumerable<Point> source, Func<(int x, int y), bool> isWall) =>
            source.TakeWhile(p => !isWall((p.X, p.Y))).Take(MaxSteps);

        public Point Middle { get; }
        public IReadOnlyCollection<Point> Left { get; }
        public IReadOnlyCollection<Point> Right { get; }
        public IReadOnlyCollection<Point> WholeLine { get; }
        public int Count => 1 + Left.Count + Right.Count;

        public PerpendicularClearance(Point middle, IReadOnlyCollection<Point> left, IReadOnlyCollection<Point> right)
        {
            Middle = middle;
            Left = left;
            Right = right;
            WholeLine = left.Concat(middle.Yield()).Concat(right).ToList();
        }
    }
}