namespace NBWTA.ResultPersister
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Result;
    using Utils;

    internal static class FilePersister
    {
        private const char Separator = '$';
        public static void Save(AnalyzedMap data, string filePath)
        {
            var regionIds = data.MapRegions.Concat<RegionBase>(data.ChokeRegions)
                .Select((r, idx) => (region: r, idx: idx + 1)).ToDictionary(x => x.region, x => x.idx);

            // [regionId][outlineTiles][contentTiles][neighborIds][resourceSites([minerals][geysers][depot])]
            var regions = data.MapRegions.Select(r =>
                (regionId: regionIds[r],
                    outline: r.OutlineTiles,
                    content: r.ContentTiles,
                    neighbors: r.AdjacentChokes.Select(ch => regionIds[ch]).ToList(),
                    sites: r.ResourceSites)).ToList();

            // [chokeId][outlineTiles][contentTiles][neighborIds][minWidthTiles][avgWidth]
            var chokes = data.ChokeRegions.Select(ch =>
                (chokeId: regionIds[ch],
                    outline: ch.OutlineTiles,
                    content: ch.ContentTiles,
                    neighbors: ch.AdjacentRegions.Select(r => regionIds[r]).ToList(),
                    minWidth: ch.MinWidthWalkTilesLine,
                    avgWidth: ch.AverageWidthInWalkTiles)).ToList();

            var regionsSerialized = JsonConvert.SerializeObject(regions);
            var chokesSerialized = JsonConvert.SerializeObject(chokes);
            var content = AssemblyVersion.Get() + Separator + regionsSerialized + Separator + chokesSerialized;

            File.WriteAllBytes(filePath, StringCompressor.Compress(content));
        }

        public static Result<AnalyzedMap> TryLoad(string filePath)
        {
            var failResult = new Result<AnalyzedMap>(null);
            if (!File.Exists(filePath)) return failResult;

            try
            {
                var fileContent = File.ReadAllBytes(filePath);
                var text = StringCompressor.Decompress(fileContent);
                var split = text.Split(Separator);
                var version = split[0];
                if (version != AssemblyVersion.Get()) return failResult;

                var regions = JsonConvert.DeserializeObject<
                    List<(int regionId, IReadOnlyCollection<(int x, int y)> outline,
                        HashSet<(int x, int y)> content, List<int> neighbors,
                        IReadOnlyCollection<ResourceSite> sites)>
                >(split[1]);

                var chokes = JsonConvert.DeserializeObject<
                    List<(int chokeId, IReadOnlyCollection<(int x, int y)> outline,
                        HashSet<(int x, int y)> content, List<int> neighbors,
                        IReadOnlyCollection<(int x, int y)> minWidth, double avgWidth)>
                >(split[2]);

                var mapRegions = regions.ToDictionary(r => r.regionId, r => new MapRegion(r.content, r.outline));
                var chokeRegions = chokes.ToDictionary(ch => ch.chokeId, ch => new ChokeRegion(ch.content, ch.outline, ch.minWidth, ch.avgWidth));

                regions.ForEach(r =>
                {
                    var mapRegion = mapRegions[r.regionId];
                    mapRegion.AdjacentChokes = r.neighbors.Select(n => chokeRegions[n]).ToList();
                    mapRegion.ResourceSites = r.sites;
                });

                chokes.ForEach(ch =>
                    chokeRegions[ch.chokeId].AdjacentRegions = ch.neighbors.Select(n => mapRegions[n]).ToList());

                var analyzedMap = new AnalyzedMap(mapRegions.Values, chokeRegions.Values);
                return new Result<AnalyzedMap>(analyzedMap);
            }
            catch (Exception)
            {
                return failResult;
            }
        }
    }
}