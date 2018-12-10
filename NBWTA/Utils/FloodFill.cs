namespace NBWTA.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Graph;

    public static class FloodFill
    {
        internal static IEnumerable<Node> Naive(Node start)
        {
            var openList = new HashSet<Node> { start };
            var closedList = new HashSet<Node>();

            while (openList.Any())
            {
                var nodeToBeProcessed = openList.First();
                openList.Remove(nodeToBeProcessed);
                closedList.Add(nodeToBeProcessed);

                yield return nodeToBeProcessed;

                openList.UnionWith(nodeToBeProcessed.Neighbors.Where(n => !closedList.Contains(n)));
            }
        }

        /// <summary>
        /// Scanline fill copied from internet..ewww
        /// </summary>
        public static IEnumerable<(int x, int y)> Scanline(int x, int y, int width, int height, Func<int, int, bool> shouldFill)
        {
            if (!shouldFill(x, y)) throw new InvalidOperationException("Start must not be invalid");
            var processed = new bool[width, height];
            yield return (x, y);
            processed[x, y] = true;
            bool Test(int xx, int yy) => !processed[xx, yy] && shouldFill(xx, yy);

            var stack = new Stack<Segment>();
            stack.Push(new Segment(x, x + 1, y, 0, true, true));
            do
            {
                var r = stack.Pop();
                int startX = r.StartX, endX = r.EndX;
                if (r.ScanLeft) // if we should extend the segment towards the left...
                {
                    while (startX > 0 && Test(startX - 1, r.Y))
                    {
                        yield return (--startX, r.Y); // do so, and fill cells as we go
                        processed[startX, r.Y] = true;
                    }
                }
                if (r.ScanRight)
                {
                    while (endX < width && Test(endX, r.Y))
                    {
                        yield return (endX++, r.Y);
                        processed[endX - 1, r.Y] = true;
                    }
                }
                // at this point, the segment from startX (inclusive) to endX (exclusive) is filled. compute the region to ignore
                r.StartX--; // since the segment is bounded on either side by filled cells or array edges, we can extend the size of
                r.EndX++;   // the region that we're going to ignore in the adjacent lines by one
                            // scan above and below the segment and add any new segments we find
                if (r.Y > 0)
                    foreach (var xy in AddLine(Test, stack, startX, endX, r.Y - 1, r.StartX, r.EndX, -1, r.Dir <= 0))
                    {
                        yield return xy;
                        processed[xy.x, xy.y] = true;
                    }
                if (r.Y < height - 1)
                    foreach (var xy in AddLine(Test, stack, startX, endX, r.Y + 1, r.StartX, r.EndX, 1, r.Dir >= 0))
                    {
                        yield return xy;
                        processed[xy.x, xy.y] = true;
                    }
            } while (stack.Count != 0);
        }

        private struct Segment
        {
            public Segment(int startX, int endX, int y, sbyte dir, bool scanLeft, bool scanRight)
            {
                StartX = startX;
                EndX = endX;
                Y = y;
                Dir = dir;
                ScanLeft = scanLeft;
                ScanRight = scanRight;
            }
            public int StartX, EndX, Y;
            public readonly sbyte Dir; // -1:above the previous segment, 1:below the previous segment, 0:no previous segment
            public readonly bool ScanLeft, ScanRight;
        }

        private static IEnumerable<(int x, int y)> AddLine(Func<int, int, bool> shouldFill, Stack<Segment> stack, int startX, int endX, int y,
                            int ignoreStart, int ignoreEnd, sbyte dir, bool isNextInDir)
        {
            int regionStart = -1, x;
            for (x = startX; x < endX; x++) // scan the width of the parent segment
            {
                if ((isNextInDir || x < ignoreStart || x >= ignoreEnd) && shouldFill(x, y)) // if we're outside the region we
                {                                                                      // should ignore and the cell is clear
                    yield return (x, y); // fill the cell
                    if (regionStart < 0) regionStart = x; // and start a new segment if we haven't already
                }
                else if (regionStart >= 0) // otherwise, if we shouldn't fill this cell and we have a current segment...
                {
                    stack.Push(new Segment(regionStart, x, y, dir, regionStart == startX, false)); // push the segment
                    regionStart = -1; // and end it
                }
                if (!isNextInDir && x < ignoreEnd && x >= ignoreStart) x = ignoreEnd - 1; // skip over the ignored region
            }
            if (regionStart >= 0) stack.Push(new Segment(regionStart, x, y, dir, regionStart == startX, true));
        }
    }
}