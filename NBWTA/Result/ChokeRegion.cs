namespace NBWTA.Result
{
    using System.Collections.Generic;

    public class ChokeRegion : RegionBase
    {
        public ChokeRegion(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles) : base(contentTiles, outlineTiles)
        {
        }

        public IReadOnlyCollection<MapRegion> AdjacentRegions { get; internal set; }
    }
}