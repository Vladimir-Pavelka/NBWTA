# NBWTA
Brood War Terrain Analyzer written in .NET.

Given a map with walkability info, detects choked areas (in red) and regions (areas enclosed by walls and chokes).

Map | | Analyzed
:-------------------------:|:-------------------------:|:-------------------------:
<img src="https://github.com/Vladimir-Pavelka/NBWTA/blob/master/Tests/BwMaps/CircuitBreaker/CircuitBreaker_walls.bmp" width="350" />  |       ----->     |<img src="https://github.com/Vladimir-Pavelka/NBWTA/blob/master/Tests/MapAnalysisResults/CircuitBreaker.bmp" width="350" />



## Usage
1. Install the [NBWTA](https://www.nuget.org/packages/NBWTA/) nuget package.
2. Using the data provided by [BWAPI](https://bwapi.github.io/), analyze the map.
```csharp
var mapWidthWalkTiles = Game.MapWidth * 4;
var mapHeightWalkTiles = Game.MapHeight * 4;
bool IsTileWalkable((int x, int y) tile) => Game.IsWalkable(tile.x, tile.x);

var mapAnalyzer = new MapAnalyzer();
AnalyzedMap analyzedMap = mapAnalyzer.Analyze(mapWidthWalkTiles, mapHeightWalkTiles, IsTileWalkable);
```
The result of type [`AnalyzedMap`](https://github.com/Vladimir-Pavelka/NBWTA/blob/master/NBWTA/Result/AnalyzedMap.cs) holds the information about found choke points ([`ChokeRegion`](https://github.com/Vladimir-Pavelka/NBWTA/blob/master/NBWTA/Result/ChokeRegion.cs)) and map regions ([`MapRegion`](https://github.com/Vladimir-Pavelka/NBWTA/blob/master/NBWTA/Result/MapRegion.cs)), alongside the functionality to load and persist this to a file.
```csharp
public class AnalyzedMap
{
    public IReadOnlyCollection<ChokeRegion> ChokeRegions { get; }
    public IReadOnlyCollection<MapRegion> MapRegions { get; }

    public void SaveToFile(string filePath);
    public static Result<AnalyzedMap> TryLoadFromFile(string filePath);
}
```

The API of [NBWTA](https://www.nuget.org/packages/NBWTA/) was designed this way so that it's independent from [BWAPI](https://bwapi.github.io/) version changes.
