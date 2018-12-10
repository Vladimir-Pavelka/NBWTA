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
}