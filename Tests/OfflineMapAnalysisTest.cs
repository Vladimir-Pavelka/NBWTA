namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using NBWTA;
    using NBWTA.Result;
    using NBWTA.Utils;
    using Xunit;
    using Xunit.Abstractions;

    public class OfflineMapAnalysisTest
    {
        private const string BwMapsFolder = "../../BwMaps";

        private const string AstralBalance = BwMapsFolder + "/AstralBalance/AstralBalance_walls.bmp";
        private const string CircuitBreaker = BwMapsFolder + "/CircuitBreaker/CircuitBreaker_walls.bmp";
        private const string Hunters = BwMapsFolder + "/TheHunters/TheHunters_walls.bmp";
        private const string LostTemple = BwMapsFolder + "/TheLostTemple/TheLostTemple_walls.bmp";
        private const string FightingSpirit = BwMapsFolder + "/FightingSpirit/FightingSpirit_walls.bmp";
        private const string Homeworld = BwMapsFolder + "/Homeworld/Homeworld_walls.bmp";
        private const string Python = BwMapsFolder + "/iCCupPython1.3/iCCupPython1.3_walls.bmp";
        private const string CrescentMoon = BwMapsFolder + "/CrescentMoon/CrescentMoon_walls.bmp";
        private const string BloodBath = BwMapsFolder + "/BloodBath/BloodBath_walls.bmp";
        private const string OrbitalGully = BwMapsFolder + "/OrbitalGully/OrbitalGully_walls.bmp";
        private const string AlphaDraconis = BwMapsFolder + "/AlphaDraconisVolcanicWorld/AlphaDraconisVolcanicWorld_walls.bmp";
        private const string Bottleneck = BwMapsFolder + "/Bottleneck/Bottleneck_walls.bmp";
        private const string Heartwood = BwMapsFolder + "/Heartwood/Heartwood_walls.bmp";
        private const string PrimevalIsles = BwMapsFolder + "/PrimevalIsles/PrimevalIsles_walls.bmp";
        private const string RiverCrossing = BwMapsFolder + "/RiverCrossing/RiverCrossing_walls.bmp";

        private const string ResultFolder = "../../MapAnalysisResults/";

        private readonly ITestOutputHelper _console;

        public OfflineMapAnalysisTest(ITestOutputHelper console)
        {
            _console = console;
        }

        [Theory]
        [InlineData(nameof(AstralBalance), AstralBalance)]
        [InlineData(nameof(CircuitBreaker), CircuitBreaker)]
        [InlineData(nameof(Hunters), Hunters)]
        [InlineData(nameof(LostTemple), LostTemple)]
        [InlineData(nameof(FightingSpirit), FightingSpirit)]
        //[InlineData(nameof(Homeworld), Homeworld)]
        [InlineData(nameof(Python), Python)]
        [InlineData(nameof(CrescentMoon), CrescentMoon)]
        [InlineData(nameof(BloodBath), BloodBath)]
        [InlineData(nameof(OrbitalGully), OrbitalGully)]
        [InlineData(nameof(AlphaDraconis), AlphaDraconis)]
        [InlineData(nameof(Bottleneck), Bottleneck)]
        [InlineData(nameof(Heartwood), Heartwood)]
        //[InlineData(nameof(PrimevalIsles), PrimevalIsles)]
        [InlineData(nameof(RiverCrossing), RiverCrossing)]
        public void AnalyzeBatch(string mapName, string mapFile)
        {
            AnalyzeSingle(mapName, mapFile);
        }

        private void AnalyzeSingle(string mapName, string mapFile)
        {
            var outputFileName = mapName + ".bmp";

            var algorithm = new MapAnalyzer();
            var inputMapBitmap = new Bitmap(Image.FromFile(mapFile));

            var walkabilityMap = ToWalkabilityMap(inputMapBitmap);

            AnalyzedMap result = null;
            var elapsedMs = MeasureElapsedMs(() =>
                result = algorithm.Analyze(inputMapBitmap.Width, inputMapBitmap.Height, tile => walkabilityMap[tile.x, tile.y]));

            _console.WriteLine($"{mapName}: {elapsedMs}ms");

            FillChokesIntoBitmap(result.ChokeRegions, inputMapBitmap);
            FillRegionsIntoBitmap(result.MapRegions, inputMapBitmap);

            Directory.CreateDirectory(ResultFolder);
            inputMapBitmap.Save(ResultFolder + outputFileName);
        }

        private static long MeasureElapsedMs(Action act)
        {
            var stopwatch = Stopwatch.StartNew();
            act();
            return stopwatch.ElapsedMilliseconds;
        }

        private static bool[,] ToWalkabilityMap(Bitmap inputBitmap)
        {
            var walkabilityMap = new bool[inputBitmap.Width, inputBitmap.Height];

            for (var column = 0; column < inputBitmap.Width; column++)
                for (var row = 0; row < inputBitmap.Height; row++)
                    walkabilityMap[column, row] = IsWalkable(inputBitmap.GetPixel(column, row));

            return walkabilityMap;
        }

        private static bool IsWalkable(Color color) =>
            new[] { Color.White, Color.Transparent, Color.Empty }.Any(c => c.ToArgb() == color.ToArgb());

        private static void FillChokesIntoBitmap(IEnumerable<ChokeRegion> chokeRegions, Bitmap bitmap) =>
            chokeRegions.ForEach(x => x.ContentTiles.ForEach(n => bitmap.SetPixel(n.x, n.y, Color.Red)));

        private static void FillRegionsIntoBitmap(IEnumerable<MapRegion> mapRegions, Bitmap bitmap) =>
            mapRegions.Select((region, idx) => (region: region, color: GetPresetColor(idx)))
                .ForEach(x => x.region.OutlineTiles.ForEach(n => bitmap.SetPixel(n.x, n.y, x.color)));

        private static Color GetPresetColor(int idx)
        {
            var colorPresets = new[]
            {
                Color.Aqua, Color.Fuchsia, Color.Green, Color.Blue, Color.Lime, Color.Black,
                Color.Orange, Color.Maroon, Color.Yellow, Color.Purple, Color.Teal
            };

            return colorPresets[idx % colorPresets.Length];
        }
    }
}