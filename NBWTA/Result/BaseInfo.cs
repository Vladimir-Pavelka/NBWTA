namespace NBWTA.Result
{
    using NBWTA.Utils;
    using System.Collections.Generic;
    using System.Linq;

    public class BaseInfo
    {
        public BaseInfo(MapRegion startLocationRegion)
        {
            StartLocationRegion = startLocationRegion;

            NaturalExpansions = startLocationRegion.AdjacentChokes
                .SelectMany(ch => ch.AdjacentRegions)
                .Except(startLocationRegion.Yield())
                .Where(r => r.ResourceSites.Any())
                .ToList();

            ChokesBetweenMainAndNaturals = startLocationRegion.AdjacentChokes
                .Intersect(NaturalExpansions.SelectMany(n => n.AdjacentChokes))
                .ToList();

            var naturalsEntrances = NaturalExpansions
                .SelectMany(n => n.AdjacentChokes)
                .GroupBy(ch => ch).ToList();

            OutsideEntrancesToNaturals = naturalsEntrances
                .Where(chg => chg.Count() == 1)
                .Select(chg => chg.Key)
                .Except(ChokesBetweenMainAndNaturals)
                .ToList();

            InsideConnectorsBetweenNaturals = naturalsEntrances
                .Where(chg => chg.Count() > 1)
                .Select(chg => chg.Key)
                .ToList();
        }

        /// <summary>
        /// Region representing the main base start location
        /// </summary>
        public MapRegion StartLocationRegion { get; }

        /// <summary>
        /// MapRegions containing resources, adjacent to the StartLocationRegion
        /// There can be 0, 1 or more NaturalExpansions, based on the map shape.
        /// </summary>
        public IReadOnlyCollection<MapRegion> NaturalExpansions { get; }
        public IReadOnlyCollection<ChokeRegion> ChokesBetweenMainAndNaturals { get; }
        public IReadOnlyCollection<ChokeRegion> OutsideEntrancesToNaturals { get; }
        public IReadOnlyCollection<ChokeRegion> InsideConnectorsBetweenNaturals { get; }
    }
}
