namespace NBWTA.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Clustering
    {
        public static IReadOnlyCollection<HashSet<TElem>> Cluster<TElem>(IReadOnlyCollection<TElem> elements, Func<TElem, TElem, bool> areInRelation)
        {
            var sets = ForEachElementGetNeighborsInRelation(elements, areInRelation);
            return ForEachElementUnionSetsWhichContainIt(elements, sets);
        }

        private static IReadOnlyCollection<HashSet<TElem>> ForEachElementGetNeighborsInRelation<TElem>(IReadOnlyCollection<TElem> allElements, Func<TElem, TElem, bool> areInRelation)
        {
            return allElements.Select(element => GetElementsInRelation(element, allElements, areInRelation)).ToList();
        }

        private static HashSet<TElem> GetElementsInRelation<TElem>(TElem origin, IEnumerable<TElem> allElements, Func<TElem, TElem, bool> areInRelation)
        {
            var elementsInRelation = allElements.Where(point => areInRelation(point, origin));
            return new HashSet<TElem>(elementsInRelation);
        }

        private static IReadOnlyCollection<HashSet<TElem>> ForEachElementUnionSetsWhichContainIt<TElem>(IEnumerable<TElem> elements, IReadOnlyCollection<HashSet<TElem>> sets)
        {
            return elements.Aggregate(sets, (accu, point) => UnionSetsWhichContainPoint(point, accu)).ToList();
        }

        private static IReadOnlyCollection<HashSet<TElem>> UnionSetsWhichContainPoint<TElem>(TElem element, IReadOnlyCollection<HashSet<TElem>> allSets)
        {
            var setsToBeUnioned = allSets.Where(set => set.Contains(element)).ToList();
            var unionedSet = UnionSets(setsToBeUnioned);

            return allSets.Except(setsToBeUnioned).Concat(new[] { unionedSet }).ToList();
        }

        private static HashSet<TElem> UnionSets<TElem>(IEnumerable<HashSet<TElem>> setsToBeUnioned)
        {
            return setsToBeUnioned.Aggregate(new HashSet<TElem>(), (accu, set) =>
            {
                set.ForEach(point => accu.Add(point));
                return accu;
            });
        }
    }
}