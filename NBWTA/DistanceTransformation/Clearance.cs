namespace NBWTA.DistanceTransformation
{
    using System.Collections.Generic;
    using System.Linq;

    public class Clearance
    {
        public int Top { get; set; }
        public int TopRight { get; set; }
        public int Right { get; set; }
        public int BotRight { get; set; }
        public int Bot { get; set; }
        public int BotLeft { get; set; }
        public int Left { get; set; }
        public int TopLeft { get; set; }

        public IEnumerable<int> AllDirections
        {
            get
            {
                yield return Top;
                yield return TopRight;
                yield return Right;
                yield return BotRight;
                yield return Bot;
                yield return BotLeft;
                yield return Left;
                yield return TopLeft;
            }
        }

        public int Min() => AllDirections.Min();
        public int Max() => AllDirections.Max();
    }
}