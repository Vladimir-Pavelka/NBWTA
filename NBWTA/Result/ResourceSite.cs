namespace NBWTA.Result
{
    using System.Collections.Generic;

    public class ResourceSite
    {
        public ResourceSite(IReadOnlyCollection<(int x, int y)> mineralsBuildTiles,
            IReadOnlyCollection<(int x, int y)> geysersBuildTiles,
            (int x, int y) optimalResourceDepotBuildTile)
        {
            MineralsBuildTiles = mineralsBuildTiles;
            GeysersBuildTiles = geysersBuildTiles;
            OptimalResourceDepotBuildTile = optimalResourceDepotBuildTile;
        }

        public IReadOnlyCollection<(int x, int y)> MineralsBuildTiles { get; }
        public IReadOnlyCollection<(int x, int y)> GeysersBuildTiles { get; }

        public (int x, int y) OptimalResourceDepotBuildTile { get; }
    }
}