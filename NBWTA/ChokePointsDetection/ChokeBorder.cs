namespace NBWTA.ChokePointsDetection
{
    using DistanceTransformation;
    using Graph;

    public class ChokeBorder
    {
        public Node Start { get; }
        public Node End { get; }
        public int Length { get; }
        public PerpendicularClearance StartBorder { get; }
        public PerpendicularClearance EndBorder { get; }

        public ChokeBorder(Node start, Node end, int length, PerpendicularClearance startBorder, PerpendicularClearance endBorder)
        {
            Start = start;
            End = end;
            Length = length;
            StartBorder = startBorder;
            EndBorder = endBorder;
        }
    }
}