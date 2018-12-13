namespace NBWTA
{
    using System;
    using System.Collections.Generic;
    using Result;

    public interface IMapAnalyzer
    {
        AnalyzedMap Analyze(int widthWalkTiles, int heightWalkTiles, Func<(int x, int y), bool> isWalkTileWalkable,
            IReadOnlyCollection<(int x, int y)> mineralsToConsiderBuildTiles = null,
            IReadOnlyCollection<(int x, int y)> geysersToConsiderBuildTiles = null,
            Func<(int x, int y), bool> isBuildTileBuildable = null);
    }
}