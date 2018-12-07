namespace NBWTA.ChokePointsDetection
{
    using System.Collections.Generic;
    using Graph;

    public class Choke
    {
        public ChokeBorder Border { get; }
        public IReadOnlyCollection<Node> Fill { get; }

        public Choke(ChokeBorder border, IReadOnlyCollection<Node> fill)
        {
            Border = border;
            Fill = fill ?? new Node[0];
        }
    }
}