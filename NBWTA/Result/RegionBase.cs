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

        public HashSet<(int x, int y)> ContentTiles { get; }
        public IReadOnlyCollection<(int x, int y)> OutlineTiles { get; }
    }
}