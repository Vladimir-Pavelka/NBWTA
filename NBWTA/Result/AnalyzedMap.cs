namespace NBWTA.Result
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ResultPersister;

    public class AnalyzedMap
    {
        public AnalyzedMap(IReadOnlyCollection<MapRegion> mapRegions, IReadOnlyCollection<ChokeRegion> chokeRegions)
        {
            MapRegions = mapRegions;
            ChokeRegions = chokeRegions;
            ResourceRegions = mapRegions.Where(mr => mr.ResourceSites.Any()).ToList();
        }

        public IReadOnlyCollection<MapRegion> MapRegions { get; }
        public IReadOnlyCollection<ChokeRegion> ChokeRegions { get; }
        public IReadOnlyCollection<MapRegion> ResourceRegions { get; }

        public BaseInfo GetBaseInfoByStartLocationWalkTile((int x, int y) startLocationWalkTile)
        {
            var startLocationRegion = MapRegions.FirstOrDefault(r => r.ContentTiles.Contains(startLocationWalkTile));
            if (startLocationRegion != null) return new BaseInfo(startLocationRegion);
            throw new ArgumentException($"startLocationWalkTile: {startLocationWalkTile} was not found in any MapRegion", nameof(startLocationWalkTile));
        }

        public void SaveToFile(string filePath) => FilePersister.Save(this, filePath);
        public static Result<AnalyzedMap> TryLoadFromFile(string filePath) => FilePersister.TryLoad(filePath);
    }
}   