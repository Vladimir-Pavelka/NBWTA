namespace NBWTA.Result
{
    using System.Collections.Generic;
    using ResultPersister;

    public class AnalyzedMap
    {
        public AnalyzedMap(IReadOnlyCollection<MapRegion> mapRegions, IReadOnlyCollection<ChokeRegion> chokeRegions)
        {
            MapRegions = mapRegions;
            ChokeRegions = chokeRegions;
        }

        public IReadOnlyCollection<MapRegion> MapRegions { get; }
        public IReadOnlyCollection<ChokeRegion> ChokeRegions { get; }

        public void SaveToFile(string filePath) => FilePersister.Save(this, filePath);
        public static Result<AnalyzedMap> TryLoadFromFile(string filePath) => FilePersister.TryLoad(filePath);
    }
}