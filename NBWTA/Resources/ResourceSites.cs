namespace NBWTA.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Result;
    using Utils;

    internal static class ResourceSites
    {
        private const int ResourceDistanceTresholdBuildTiles = 6;

        public static IEnumerable<ResourceSite> Analyze(int mapWidth, int mapHeight,
            IReadOnlyCollection<(int x, int y)> mineralsToConsiderBuildTiles,
            IReadOnlyCollection<(int x, int y)> geysersToConsiderBuildTiles,
            Func<(int x, int y), bool> isBuildTileBuildable,
            IDictionary<(int x, int y), MapRegion> buildTileRegionMap)
        {
            var geysers = geysersToConsiderBuildTiles.ToHashSet();
            var resourceClusters = mineralsToConsiderBuildTiles.Concat(geysersToConsiderBuildTiles).ToList()
                .Cluster((a, b) => Distances.Euclidean(a, b) < ResourceDistanceTresholdBuildTiles);

            bool IsMineral((int x, int y) r) => !geysers.Contains(r);
            bool IsGeyser((int x, int y) r) => geysers.Contains(r);

            var resourceDepotPlacements = ResourceDepotPlacement.CalculatePlacements(mapWidth, mapHeight,
                resourceClusters, geysers, isBuildTileBuildable, buildTileRegionMap);

            return resourceClusters.Zip(resourceDepotPlacements,
                (cluster, depotPlacement) => new ResourceSite(cluster.Where(IsMineral).ToList(),
                    cluster.Where(IsGeyser).ToList(), depotPlacement));
        }
    }
}