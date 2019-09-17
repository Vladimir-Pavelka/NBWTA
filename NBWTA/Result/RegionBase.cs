namespace NBWTA.Result
{
    using System.Collections.Generic;

    public abstract class RegionBase
    {
        protected RegionBase(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles)
        {
            ContentTiles = contentTiles ?? new HashSet<(int x, int y)>();
            OutlineTiles = outlineTiles ?? new List<(int x, int y)>();
        }

        /// <summary>
        /// Walktiles inside this region (including OutlineTiles). 
        /// </summary>
        public HashSet<(int x, int y)> ContentTiles { get; }

        /// <summary>
        /// Border walktiles at the edge of this region.
        /// </summary>
        public IReadOnlyCollection<(int x, int y)> OutlineTiles { get; }
    }
}