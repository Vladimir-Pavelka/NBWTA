namespace NBWTA.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Result;
    using Utils;

    internal class ResourceDepotPlacement
    {
        private const int MaxSearchAreaTiles = 10;
        private const int MinDistanceFromResourceTiles = 3;
        private const int MainBuildingWidthTiles = 4;
        private const int MainBuildingHeightTiles = 3;
        private const int MineralWidthTiles = 2;
        private const int MineralHeightTiles = 1;
        private const int GeyserWidthTiles = 4;
        private const int GeyserHeightTiles = 2;

        internal static IReadOnlyCollection<(int x, int y)> CalculatePlacements(int mapWidthTiles, int mapHeightTiles,
            IReadOnlyCollection<HashSet<(int x, int y)>> resourceSites, HashSet<(int x, int y)> geysers,
            Func<(int x, int y), bool> isBuildable, IDictionary<(int x, int y), MapRegion> buildTileRegionMap)
        {
            return resourceSites.Select(site =>
                CalculatePlacementForSingleSite(mapWidthTiles, mapHeightTiles, site, geysers, isBuildable, buildTileRegionMap)).ToList();
        }

        internal static (int x, int y) CalculatePlacementForSingleSite(int mapWidthTiles, int mapHeightTiles,
            HashSet<(int x, int y)> resourceSite, HashSet<(int x, int y)> geysers,
            Func<(int x, int y), bool> isBuildable, IDictionary<(int x, int y), MapRegion> buildTileRegionMap)
        {
            var allResourceXCoordinates = resourceSite.Select(resource => resource.x).ToList();
            var allResourceYCoordinates = resourceSite.Select(resource => resource.y).ToList();

            var searchBoxMinX = GetMinXWithinMapBoundary(allResourceXCoordinates.Min() - MaxSearchAreaTiles);
            var searchBoxMinY = GetMinYWithinMapBoundary(allResourceYCoordinates.Min() - MaxSearchAreaTiles);
            var searchBoxMaxX = GetMaxXWithinMapBoundary(allResourceXCoordinates.Max() + MaxSearchAreaTiles, mapWidthTiles);
            var searchBoxMaxY = GetMaxYWithinMapBoundary(allResourceYCoordinates.Max() + MaxSearchAreaTiles, mapHeightTiles);

            var searchBoxTiles = YieldSearchBoxTiles(searchBoxMinX, searchBoxMaxX, searchBoxMinY, searchBoxMaxY);

            var mineralTopLefts = resourceSite.Where(x => !geysers.Contains(x)).ToList();
            var geyserTopLefts = resourceSite.Where(x => geysers.Contains(x)).ToList();

            var mineralsOcupancy = mineralTopLefts.SelectMany(mineral => YieldResourceOccupancy(mineral, MineralWidthTiles, MineralHeightTiles)).Distinct();
            var geysersOccupancy = geyserTopLefts.SelectMany(geyser => YieldResourceOccupancy(geyser, GeyserWidthTiles, GeyserHeightTiles)).Distinct();

            var resourcesOccupancy = mineralsOcupancy.Concat(geysersOccupancy).Distinct();

            var buildableTiles = searchBoxTiles.Except(resourcesOccupancy).Where(isBuildable).ToList();
            var fullBodyMinerals = mineralTopLefts.SelectMany(x => ToFullBodyResource(x, MineralWidthTiles, MineralHeightTiles));
            var fullBodyGeysers = geyserTopLefts.SelectMany(x => ToFullBodyResource(x, GeyserWidthTiles, GeyserHeightTiles));

            var scoredBuildableTiles = buildableTiles.Select(tile => CalculateTileScore(tile, fullBodyMinerals, fullBodyGeysers)).ToDictionary(x => x.Tile, x => x.Score);

            var placementsOrderedByScore = buildableTiles
                .Select(tile => CalculateBuildingPlacementScore(tile, scoredBuildableTiles))
                .OrderBy(x => x.Score)
                .Select(x => x.Tile)
                .ToList();

            bool AreInSameRegion((int x, int y) left, (int x, int y) right) =>
                buildTileRegionMap.TryGetValue(left, out var leftRegion) &&
                buildTileRegionMap.TryGetValue(right, out var rightRegion) && leftRegion == rightRegion;

            return placementsOrderedByScore.FirstOrNull(placement => AreInSameRegion(placement, resourceSite.First())) ??
                   placementsOrderedByScore.First();
        }

        private static ((int x, int y) Tile, double Score) CalculateBuildingPlacementScore((int x, int y) tile, IReadOnlyDictionary<(int x, int y), double> scoredBuildableTiles)
        {
            var xRange = Enumerable.Range(tile.x, MainBuildingWidthTiles);
            var yRange = Enumerable.Range(tile.y, MainBuildingHeightTiles);
            var buildingBodyTiles = xRange.SelectMany(x => yRange.Select(y => (x, y))).ToList();

            if (!buildingBodyTiles.All(scoredBuildableTiles.ContainsKey))
            {
                return (tile, double.MaxValue);
            }

            var positionScore = buildingBodyTiles.Select(x => scoredBuildableTiles[x]).Sum();
            return (tile, positionScore);
        }

        private static ((int x, int y) Tile, double Score) CalculateTileScore((int x, int y) tile, IEnumerable<(int x, int y)> mineralTiles, IEnumerable<(int x, int y)> geyserTiles)
        {
            var score = mineralTiles.Concat(geyserTiles).Select(resource => Distance(resource, tile)).Average();

            return (tile, score);
        }

        private static double Distance((int x, int y) left, (int x, int y) right)
        {
            return Distances.Euclidean(left, right);
        }

        private static IEnumerable<(int x, int y)> YieldSearchBoxTiles(int searchBoxMinX, int searchBoxMaxX, int searchBoxMinY, int searchBoxMaxY)
        {
            for (var x = searchBoxMinX; x <= searchBoxMaxX; x++)
                for (var y = searchBoxMinY; y <= searchBoxMaxY; y++)
                    yield return (x, y);
        }

        private static IEnumerable<(int x, int y)> ToFullBodyResource((int x, int y) resourceTopLeft, int resourceWidth, int resourceHeight)
        {
            for (var x = resourceTopLeft.x; x <= resourceTopLeft.x + resourceWidth - 1; x++)
                for (var y = resourceTopLeft.y; y <= resourceTopLeft.y + resourceHeight - 1; y++)
                    yield return (x, y);
        }

        private static IEnumerable<(int x, int y)> YieldResourceOccupancy((int x, int y) resourceTopLeft, int resourceWidth, int resourceHeight)
        {
            for (var x = resourceTopLeft.x - MinDistanceFromResourceTiles; x <= resourceTopLeft.x + resourceWidth - 1 + MinDistanceFromResourceTiles; x++)
                for (var y = resourceTopLeft.y - MinDistanceFromResourceTiles; y <= resourceTopLeft.y + resourceHeight - 1 + MinDistanceFromResourceTiles; y++)
                    yield return (x, y);
        }

        private static int GetMinXWithinMapBoundary(int potentialMinX) => potentialMinX < 0 ? 0 : potentialMinX;
        private static int GetMinYWithinMapBoundary(int potentialMinY) => potentialMinY < 0 ? 0 : potentialMinY;

        private static int GetMaxXWithinMapBoundary(int potentialMaxX, int mapWidthTiles) => potentialMaxX >= mapWidthTiles ? mapWidthTiles - 1 : potentialMaxX;
        private static int GetMaxYWithinMapBoundary(int potentialMaxY, int mapHeightTiles) => potentialMaxY >= mapHeightTiles ? mapHeightTiles - 1 : potentialMaxY;
    }
}