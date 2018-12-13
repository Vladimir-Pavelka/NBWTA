namespace NBWTA.Result
{
    using System.Collections.Generic;

    public class MapRegion : RegionBase
    {
        public MapRegion(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles) : base(contentTiles, outlineTiles)
        {
        }

        public IReadOnlyCollection<ChokeRegion> AdjacentChokes { get; internal set; } = new List<ChokeRegion>();
        public IReadOnlyCollection<ResourceSite> ResourceSites { get; internal set; } = new List<ResourceSite>();
    }
}