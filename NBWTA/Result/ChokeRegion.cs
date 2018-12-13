namespace NBWTA.Result
{
    using System.Collections.Generic;

    public class ChokeRegion : RegionBase
    {
        internal ChokeRegion(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles,
            IReadOnlyCollection<(int x, int y)> minWidthWalkTilesLine, double averageWidthInWalkTiles) : base(
            contentTiles, outlineTiles)
        {
            MinWidthWalkTilesLine = minWidthWalkTilesLine;
            AverageWidthInWalkTiles = averageWidthInWalkTiles;
        }

        public IReadOnlyCollection<MapRegion> AdjacentRegions { get; internal set; }
        public IReadOnlyCollection<(int x, int y)> MinWidthWalkTilesLine { get; }
        public double AverageWidthInWalkTiles { get; }
    }
}