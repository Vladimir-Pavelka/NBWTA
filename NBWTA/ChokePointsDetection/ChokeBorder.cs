namespace NBWTA.ChokePointsDetection
{
    using DistanceTransformation;
    using Graph;

    internal class ChokeBorder
    {
        public Node Start { get; }
        public Node End { get; }
        public int Length { get; }
        public double AvgWidth { get; }
        public PerpendicularClearance StartBorder { get; }
        public PerpendicularClearance EndBorder { get; }
        public PerpendicularClearance NarrowestSection { get; }

        public ChokeBorder(Node start, Node end, int length, double avgWidth, PerpendicularClearance startBorder,
            PerpendicularClearance endBorder, PerpendicularClearance narrowestSection)
        {
            Start = start;
            End = end;
            Length = length;
            AvgWidth = avgWidth;
            StartBorder = startBorder;
            EndBorder = endBorder;
            NarrowestSection = narrowestSection;
        }
    }
}