namespace NBWTA.ChokePointsDetection
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using DistanceTransformation;
    using Graph;
    using RegionDetection;
    using Utils;

    internal static class ChokePointsDetector
    {
        public static IReadOnlyCollection<ChokeBorder> GetChokeBorders(IReadOnlyCollection<Vertex> connectedGraph, Func<(int x, int y), bool> isWall)
        {
            var graphEdges = connectedGraph.SelectMany(v => v.Edges).Distinct().ToList();
            var perpendicularClearance = GetPerpendicularClearance(graphEdges, isWall);
            var chokeBorders = graphEdges.SelectMany(e => DetectChokeBorders(e, perpendicularClearance)).ToList();
            return chokeBorders;
        }

        // TODO: needs work: extract class, refactor code
        private static IDictionary<Node, PerpendicularClearance> GetPerpendicularClearance(IEnumerable<Edge> connections, Func<(int x, int y), bool> isWall)
        {
            var paths = connections.Select(conn => conn.Path);
            const int lineLength = 9;
            return paths
                .Where(path => path.Count >= lineLength)
                .Select(path => Prolong(path, lineLength))
                .SelectMany(path =>
                    path.SlidingWindow(lineLength).Select(buff =>
                        (node: buff.Middle(), clearance: PerpendicularClearance.Get(buff.Select(n => n.ToPoint()).ToList(), buff.Middle().ToPoint(), isWall)))
                ).ToDictionary(x => x.node, x => x.clearance);
        }

        private static IReadOnlyCollection<Node> Prolong(IReadOnlyCollection<Node> path, int lineLength)
        {
            var startPoints = path.Take(lineLength).Select(n => n.ToPoint()).ToArray();
            var startLine = Line.FromPoints(startPoints);
            var startPoint = startLine.GetClosestLinePointTo(startPoints.First());
            var startProlongedLeft = startLine.LeftIndexesFrom(startPoint).Take(lineLength / 2).ToList();
            var startProlongedRight = startLine.RightIndexesFrom(startPoint).Take(lineLength / 2).ToList();
            var prolongedStart = new[] { startProlongedLeft, startProlongedRight }.OrderBy(x => Distance(x.Last(), startPoints.Last())).Last();

            var endPoints = path.Skip(path.Count - lineLength).Select(n => n.ToPoint()).ToArray();
            var endLine = Line.FromPoints(endPoints);
            var endPoint = endLine.GetClosestLinePointTo(endPoints.Last());
            var endProlongedLeft = endLine.LeftIndexesFrom(endPoint).Take(lineLength / 2).ToList();
            var endProlongedRight = endLine.RightIndexesFrom(endPoint).Take(lineLength / 2).ToList();
            var prolongedEnd = new[] { endProlongedLeft, endProlongedRight }.OrderBy(x => Distance(x.Last(), endPoints.First())).Last();

            var prolonged = prolongedStart.Select(p => new Node(p.X, p.Y, true)).Concat(path)
                .Concat(prolongedEnd.Select(p => new Node(p.X, p.Y, true))).ToList();

            return prolonged;
        }

        private static double Distance(Point a, Point b)
        {
            var diffX = a.X - b.X;
            var diffY = a.Y - b.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        // TODO: needs work: extract class, refactor code
        private static IEnumerable<ChokeBorder> DetectChokeBorders(Edge edge, IDictionary<Node, PerpendicularClearance> clearanceMap)
        {
            const int minEdgeLength = 42; //44, 31
            const int maxChokeWidth = 50;
            const double chokeDiffRatio = 1.3;

            double TakeChokeAdjacentWidth(int width) =>
                width < maxChokeWidth * 0.33
                    ? width * 1.5
                    : width < maxChokeWidth * 0.9
                        ? width * 1.3
                        : width * 1.2;

            const int mergeChokesDistance = 6;

            if (edge.Path.Count < minEdgeLength) return Enumerable.Empty<ChokeBorder>();
            var localOptimas = DetectLocalOptimas(edge, clearanceMap, (s, t) => s - t < 0, (s, t) => s <= t)
                .Where(lo => clearanceMap[lo.node].WholeLine.Count < maxChokeWidth);

            localOptimas = localOptimas
               .Where(lo => edge.Path.ToList().IterateLeft(lo.idx).Skip(7).Where(clearanceMap.ContainsKey).Any(n => clearanceMap[n].Count > clearanceMap[lo.node].Count * chokeDiffRatio))
               .Where(lo => edge.Path.ToList().IterateRight(lo.idx).Skip(7).Where(clearanceMap.ContainsKey).Any(n => clearanceMap[n].Count > clearanceMap[lo.node].Count * chokeDiffRatio));

            var chokes = localOptimas.Select(lo =>
            {
                var start = edge.Path.ToList().IterateLeft(lo.idx).Where(clearanceMap.ContainsKey).TakeWhile(n =>
                    clearanceMap[n].Count < TakeChokeAdjacentWidth(clearanceMap[lo.node].Count)).Last();

                var end = edge.Path.ToList().IterateRight(lo.idx).Where(clearanceMap.ContainsKey).TakeWhile(n =>
                    clearanceMap[n].Count < TakeChokeAdjacentWidth(clearanceMap[lo.node].Count)).Last();
                return new ChokeBorder(start, end, 0, clearanceMap[start], clearanceMap[end]);
            }).ToList();

            var chokeStartEndIndices = chokes.Select(ch => GetStartEndIndices(ch, edge)).ToList();
            var intersected = chokeStartEndIndices.Cluster((a, b) =>
                    HaveIntersection(a.startIdx, a.endIdx, b.startIdx, b.endIdx) ||
                    AreCloserThan(mergeChokesDistance, a.startIdx, a.endIdx, b.startIdx, b.endIdx));

            var overlappingChokesMerged = intersected.Select(set =>
            {
                var unionStart = set.OrderBy(x => x.startIdx).First();
                var unionEnd = set.OrderByDescending(x => x.endIdx).First();
                var chokeLength = unionEnd.endIdx - unionStart.startIdx;
                var newStart = unionStart.choke.Start;
                var newEnd = unionEnd.choke.End;

                return new ChokeBorder(newStart, newEnd, chokeLength, clearanceMap[newStart], clearanceMap[newEnd]);
            });

            return overlappingChokesMerged;
        }

        private static (ChokeBorder choke, int startIdx, int endIdx) GetStartEndIndices(ChokeBorder ch, Edge edge)
        {
            var edgeNodeIndices = edge.Path.Select((n, idx) => (node: n, index: idx)).ToList();
            var startIdx = edgeNodeIndices.First(x => x.node == ch.Start).index;
            var endIdx = edgeNodeIndices.First(x => x.node == ch.End).index;
            return (ch, startIdx, endIdx);
        }

        private static bool HaveIntersection(int aStart, int aEnd, int bStart, int bEnd) => bStart <= aEnd && bEnd >= aStart;

        private static bool AreCloserThan(int treshold, int aStart, int aEnd, int bStart, int bEnd) =>
            Math.Abs(aStart - bEnd) < treshold || Math.Abs(aEnd - bStart) < treshold;

        private static IEnumerable<(Node node, int idx)> DetectLocalOptimas(Edge edge, IDictionary<Node, PerpendicularClearance> clearanceMap, Func<int, int, bool> anyPredicate, Func<int, int, bool> allPredicate)
        {
            var path = edge.Path.ToArray();
            for (var i = 0; i < path.Length; i++)
            {
                var hasClearance = clearanceMap.TryGetValue(path[i], out var clearanceNodes);
                if (!hasClearance) continue;
                var clearance = clearanceNodes.Count == 0 ? 1 : clearanceNodes.Count;
                var halfClearance = clearance / 2 + 10; // +40
                var isFarEnoughFromStart = i + 1 > halfClearance;
                var isFarEnoughFromEnd = halfClearance < path.Length - i;
                var checkRangeLeft = isFarEnoughFromStart ? halfClearance : i;
                var checkRangeRight = isFarEnoughFromEnd ? halfClearance : path.Length - i - 1;

                var indicesLeft = Enumerable.Range(i - checkRangeLeft, checkRangeLeft);
                var indicesRight = Enumerable.Range(i + 1, checkRangeRight);
                var resultLeft = DetectLocalOptimas(indicesLeft, path, clearance, clearanceMap, anyPredicate, allPredicate);
                var resultRight = DetectLocalOptimas(indicesRight, path, clearance, clearanceMap, anyPredicate, allPredicate);
                if (resultLeft.allSatisfy && resultLeft.anySatisfy &&
                    resultRight.allSatisfy && resultRight.anySatisfy)
                    yield return (path[i], i);
            }
        }

        private static (bool allSatisfy, bool anySatisfy) DetectLocalOptimas(IEnumerable<int> indicesToCheck, Node[] path, int clearance, IDictionary<Node, PerpendicularClearance> clearanceMap, Func<int, int, bool> anyPredicate, Func<int, int, bool> allPredicate)
        {
            var anySatisfied = false;
            var allSatisfied = true;

            var clearancesToCheck =
                    indicesToCheck.Select(idx => (hasSome: clearanceMap.TryGetValue(path[idx], out var clearanceToCheck), clearance: clearanceToCheck))
                .Where(x => x.hasSome)
                .Select(x => x.clearance);

            foreach (var clearanceToCheck in clearancesToCheck)
            {
                var clearanceToCheckCount = clearanceToCheck.Count == 0 ? 1 : clearanceToCheck.Count;
                if (!anySatisfied && anyPredicate(clearance, clearanceToCheckCount)) anySatisfied = true;
                if (allPredicate(clearance, clearanceToCheckCount)) continue;
                allSatisfied = false;
                break;
            }

            return (allSatisfied, anySatisfied);
        }

        public static IReadOnlyCollection<Choke> CreateFilledChokeRegions(IEnumerable<ChokeBorder> chokeBorders,
            IDictionary<ChokeBorder, (Region<Node>[] left, Region<Node>[] right)> chokeRegionMap,
            IDictionary<Region<Node>, HashSet<ChokeBorder>> regionChokeMap,
            Node[,] tileNodeMap)
        {
            var chokes = chokeBorders
                .Select(chb => new Choke(chb, GetChokeFill(chb, chokeRegionMap, regionChokeMap, tileNodeMap))).ToList();

            return chokes;
        }

        private static IReadOnlyCollection<Node> GetChokeFill(ChokeBorder chokeBorder,
            IDictionary<ChokeBorder, (Region<Node>[] left, Region<Node>[] right)> chokeRegionMap,
            IDictionary<Region<Node>, HashSet<ChokeBorder>> regionChokeMap,
            Node[,] tileNodeMap)
        {
            const int smallRegionVolume = 1000;
            const int microRegionVolume = 50;
            if (chokeBorder.StartBorder == chokeBorder.EndBorder) return chokeBorder.StartBorder.WholeLine.Select(p => tileNodeMap[p.X, p.Y]).ToList();

            var (leftNeighborRegions, rightNeighborRegions) = chokeRegionMap[chokeBorder];

            if (leftNeighborRegions.Length < 2 || rightNeighborRegions.Length < 2) return new List<Node>(); // TODO: return at least the edge?

            var regionsTouchingBothChokeLines = leftNeighborRegions.Intersect(rightNeighborRegions).ToList();
            var mikroRegionsTrappedAgainstWall = leftNeighborRegions.Union(rightNeighborRegions).Except(regionsTouchingBothChokeLines)
                .Where(r => r.Nodes.Count <= microRegionVolume);
            var smallRegionsTouchingOnlyThisChoke = regionsTouchingBothChokeLines.Where(r =>
            {
                var chokesTouchedByThisRegion = regionChokeMap[r];
                return chokesTouchedByThisRegion.Count == 1 && r.Nodes.Count <= smallRegionVolume;
            }).ToList();

            return (regionsTouchingBothChokeLines.Count <= 1 ? regionsTouchingBothChokeLines : regionsTouchingBothChokeLines.OrderBy(r => r.Nodes.Count).Take(1))
                .Union(smallRegionsTouchingOnlyThisChoke)
                .Union(mikroRegionsTrappedAgainstWall)
                .SelectMany(r => r.Nodes)
                .Union(chokeBorder.StartBorder.WholeLine.Concat(chokeBorder.EndBorder.WholeLine).Select(p => tileNodeMap[p.X, p.Y]))
                .ToList();
        }
    }
}