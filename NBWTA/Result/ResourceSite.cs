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

        /// <summary>
        /// BuildTile positions of Minerals contained by this ResourceSite
        /// </summary>
        public IReadOnlyCollection<(int x, int y)> MineralsBuildTiles { get; }

        /// <summary>
        /// BuildTile positions of Geysers contained by this ResourceSite
        /// </summary>
        public IReadOnlyCollection<(int x, int y)> GeysersBuildTiles { get; }

        /// <summary>
        /// Optimal resource depot placement for this <see cref="ResourceSite"/>.
        /// Minimum cumulative distance to minerals, while respecting the proximity restrictions. 
        /// Slightly biased towards the Geyser (if any).
        /// </summary>
        public (int x, int y) OptimalResourceDepotBuildTile { get; }
    }
}