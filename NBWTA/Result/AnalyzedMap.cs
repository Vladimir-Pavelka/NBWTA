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

        /// <summary>
        /// All identified wallkable regions.
        /// </summary>
        public IReadOnlyCollection<MapRegion> MapRegions { get; }

        /// <summary>
        /// All identified choke-point regions. (ChokeRegion connects exactly two MapRegions)
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> ChokeRegions { get; }

        /// <summary>
        /// All identified regions, which contain at least one <see cref="ResourceSite"/>
        /// </summary>
        public IReadOnlyCollection<MapRegion> ResourceRegions { get; }

        /// <summary>
        /// Retrieves information about a start location. 
        /// <paramref name="startLocationWalkTile"/> must exist inside at least one MapRegion.
        /// </summary>
        public BaseInfo GetBaseInfoByStartLocationWalkTile((int x, int y) startLocationWalkTile)
        {
            var startLocationRegion = MapRegions.FirstOrDefault(r => r.ContentTiles.Contains(startLocationWalkTile));
            if (startLocationRegion != null) return new BaseInfo(startLocationRegion);
            throw new ArgumentException($"startLocationWalkTile: {startLocationWalkTile} was not found in any MapRegion", nameof(startLocationWalkTile));
        }

        /// <summary>
        /// Serializes and stores this <see cref="AnalyzedMap"/> instance to a file, specified by the <paramref name="filePath"/>
        /// </summary>
        public void SaveToFile(string filePath) => FilePersister.Save(this, filePath);

        /// <summary>
        /// Tries to load and deserialize a <see cref="AnalyzedMap"/> instance from a file, specified by the <paramref name="filePath"/>
        /// On success, Result.HasValue == true is returned, with Result.Value containing the loaded <see cref="AnalyzedMap"/> instance.
        /// On failure, Result.HasValue == false is returned.
        /// </summary>
        public static Result<AnalyzedMap> TryLoadFromFile(string filePath) => FilePersister.TryLoad(filePath);
    }
}   