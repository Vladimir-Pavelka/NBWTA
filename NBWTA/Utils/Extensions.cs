namespace NBWTA.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Extensions
    {
        public static void ForEach<TElement>(this IEnumerable<TElement> source, Action<TElement> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void ForEach<TElement>(this TElement[,] array, Action<int, int, TElement> action)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    action(x, y, array[x, y]);
        }

        public static void ForEachReverse<TElement>(this TElement[,] array, Action<int, int, TElement> action)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            for (var x = width - 1; x >= 0; x--)
                for (var y = height - 1; y >= 0; y--)
                    action(x, y, array[x, y]);
        }

        public static IEnumerable<T> IterateLeft<T>(this IReadOnlyList<T> source, int startIndex, int? maxSteps = null)
        {
            maxSteps = maxSteps ?? int.MaxValue;
            for (var i = startIndex; i >= 0 && i > startIndex - maxSteps; i--)
                yield return source[i];
        }

        public static IEnumerable<T> IterateRight<T>(this IReadOnlyList<T> source, int startIndex, int? maxSteps = null)
        {
            maxSteps = maxSteps ?? int.MaxValue - startIndex;
            for (var i = startIndex; i < source.Count && i < startIndex + maxSteps; i++)
                yield return source[i];
        }

        public static TElem Middle<TElem>(this IReadOnlyCollection<TElem> source) => source.Skip(source.Count / 2).First();

        public static IEnumerable<TElement> Yield<TElement>(this TElement element)
        {
            yield return element;
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);

        /// <summary>
        /// Generates a sequence of chunks over the source sequence, with specified length and possible overlap.
        /// </summary>
        /// <typeparam name="TSource">Source sequence element type.</typeparam>
        /// <param name="source">Source sequence.</param>
        /// <param name="windowLength">Number of elements in each chunk</param>
        /// <param name="stepSize">Number of elements to skip between the start of consecutive window.</param>
        /// <returns>Sequence of 'windows' containing source sequence elements.</returns>
        public static IEnumerable<IReadOnlyCollection<TSource>> SlidingWindow<TSource>(this IEnumerable<TSource> source, int windowLength, int stepSize = 1)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (windowLength <= 0) throw new ArgumentOutOfRangeException(nameof(windowLength));
            if (stepSize <= 0) throw new ArgumentOutOfRangeException(nameof(stepSize));

            return source.SlidingWindowInternal(windowLength, stepSize);
        }

        private static IEnumerable<IReadOnlyCollection<TSource>> SlidingWindowInternal<TSource>(this IEnumerable<TSource> source, int windowLength, int stepSize)
        {
            var buffers = new Queue<List<TSource>>();

            var i = 0;
            foreach (var item in source)
            {
                if (i % stepSize == 0) buffers.Enqueue(new List<TSource>(windowLength));
                foreach (var buffer in buffers) buffer.Add(item);
                if (buffers.Count > 0 && buffers.Peek().Count == windowLength) yield return buffers.Dequeue();
                i++;
            }

            while (buffers.Count > 0)
            {
                var buffer = buffers.Dequeue();
                if (buffer.Count == windowLength) yield return buffer;
            }
        }

        public static IReadOnlyCollection<HashSet<TElem>> Cluster<TElem>(this IReadOnlyCollection<TElem> elements, Func<TElem, TElem, bool> areInRelation) =>
            Clustering.Cluster(elements, areInRelation);


        public static ((int x, int y) topLeft, (int x, int y) botRight) BoundingBox(this IReadOnlyCollection<(int x, int y)> points)
        {
            var minX = points.Min(p => p.x);
            var minY = points.Min(p => p.y);

            var maxX = points.Max(p => p.x);
            var maxY = points.Max(p => p.y);

            return ((minX, minY), (maxX, maxY));
        }
    }
}