namespace NBWTA.Result
{
    using System.Collections.Generic;

    public class AnalyzedMap
    {
        public AnalyzedMap(IReadOnlyCollection<MapRegion> mapRegions, IReadOnlyCollection<ChokeRegion> chokeRegions)
        {
            MapRegions = mapRegions;
            ChokeRegions = chokeRegions;
        }

        public IReadOnlyCollection<MapRegion> MapRegions { get; }
        public IReadOnlyCollection<ChokeRegion> ChokeRegions { get; }
    }

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

    public class MapRegion : RegionBase
    {
        public MapRegion(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles) : base(contentTiles, outlineTiles)
        {
        }

        public IReadOnlyCollection<ChokeRegion> AdjacentChokes { get; internal set; }
    }

    public class ChokeRegion : RegionBase
    {
        public ChokeRegion(HashSet<(int, int)> contentTiles, IReadOnlyCollection<(int, int)> outlineTiles) : base(contentTiles, outlineTiles)
        {
        }

        public IReadOnlyCollection<MapRegion> AdjacentRegions { get; internal set; }
    }
}