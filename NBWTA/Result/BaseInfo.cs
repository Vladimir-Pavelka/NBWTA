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
        /// MapRegion(s) with resources, adjacent to the StartLocationRegion
        /// There can be 0, 1 or more NaturalExpansion(s), based on the map shape (normally, there is 1).
        /// </summary>
        public IReadOnlyCollection<MapRegion> NaturalExpansions { get; }

        /// <summary>
        /// ChokeRegion(s) connecting StartLocationRegion with NaturalExpansion(s).
        /// There can be 0, 1 or more Choke(s)BetweenMainAndNatural(s), based on the map shape (normally, there is 1).
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> ChokesBetweenMainAndNaturals { get; }

        /// <summary>
        /// Adjacent ChokeRegion(s) of NaturalExpansion(s), which are not connecting to the StartLocationRegion, and are not connecting other NaturalExpansion(s) to each other (in case of multiple)
        /// There can be 0, 1 or more OutsideEntrancesToNaturals, based on the map shape (normally, there is 1).
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> OutsideEntrancesToNaturals { get; }

        /// <summary>
        /// Internal ChokeRegion connections between 
        /// There can be 0, 1 or more OutsideEntrancesToNaturals, based on the map shape (normally, there is 0).
        /// </summary>
        public IReadOnlyCollection<ChokeRegion> InsideConnectorsBetweenNaturals { get; }
    }
}
