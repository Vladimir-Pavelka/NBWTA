namespace NBWTA
{
    using System;
    using Result;

    public interface IMapAnalyzer
    {
        AnalyzedMap Analyze(int width, int height, Func<(int x, int y), bool> isWalkable);
    }
}