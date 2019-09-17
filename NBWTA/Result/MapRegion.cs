namespace NBWTA.Result
{
    using System.Collections.Generic;

    public class MapRegion : RegionBase
    {
        public MapRegion(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles) : base(contentTiles, outlineTiles)
        {
        }

        /// <summary>
        /// Choke-point regions, adjacent to this region. Can be zero, 1 or more.
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> AdjacentChokes { get; internal set; } = new List<ChokeRegion>();

        /// <summary>
        /// Resource sites - clusters, inside this region. Can contain zero, 1 or more.
        /// </summary>
        public IReadOnlyCollection<ResourceSite> ResourceSites { get; internal set; } = new List<ResourceSite>();
    }
}