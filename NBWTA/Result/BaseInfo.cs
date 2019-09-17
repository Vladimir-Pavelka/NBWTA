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
        /// Region representing the main base start location.
        /// </summary>
        public MapRegion StartLocationRegion { get; }

        /// <summary>
        /// MapRegion(s) with resources, adjacent to the StartLocationRegion.
        /// Normally there is 1, but there can be 0, 1 or more, based on the map shape.
        /// </summary>
        public IReadOnlyCollection<MapRegion> NaturalExpansions { get; }

        /// <summary>
        /// ChokeRegion(s) connecting StartLocationRegion with NaturalExpansion(s).
        /// Normally there is 1, but there can be 0, 1 or more based on the map shape.
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> ChokesBetweenMainAndNaturals { get; }

        /// <summary>
        /// ChokeRegion(s) connecting NaturalExpansion(s) to "outside" MapRegions (i.e. not StartLocationRegion, not other NaturalExpansions).
        /// Normally there is 1, but there can be 0, 1 or more based on the map shape.
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> OutsideEntrancesToNaturals { get; }

        /// <summary>
        /// Internal ChokeRegion connections between NaturalExpansions (in case of multiple).
        /// Normally there is 0, but there can be 0, 1 or more, based on the map shape.
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> InsideConnectorsBetweenNaturals { get; }
    }
}
