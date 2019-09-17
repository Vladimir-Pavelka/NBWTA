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

        /// <summary>
        /// The two MapRegions connected by this choke-point.
        /// </summary>
        public IReadOnlyCollection<MapRegion> AdjacentRegions { get; internal set; }

        /// <summary>
        /// A perpendicular line of WalkTiles, placed at the narrowest point of this choke-point.
        /// </summary>
        public IReadOnlyCollection<(int x, int y)> MinWidthWalkTilesLine { get; }

        /// <summary>
        /// The average perpendicular width of this choke-point, measured in WalkTiles.
        /// </summary>
        public double AverageWidthInWalkTiles { get; }
    }
}