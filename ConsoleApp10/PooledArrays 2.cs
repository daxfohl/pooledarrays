using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PooledArrays2
{
    public static class PooledArrayExtensions
    {
        public static PooledArray<T> ToPooledArray<T>(this IEnumerable<T> xs)
        {
            var sw = Stopwatch.StartNew();
            var array = ArrayPool<T>.Shared.Rent(0);
            var arrays = new List<T[]> { array };
            var totalSize = array.Length;
            var index = 0;
            Console.WriteLine($"req: 0, got: {array.Length}, total: 1 for {totalSize}");
            int count = 0;
            try
            {
                foreach (var x in xs)
                {
                    if (index >= array.Length)
                    {
                        var size = count == 0 ? 1 : count;
                        array = ArrayPool<T>.Shared.Rent(size);
                        arrays.Add(array);
                        totalSize += array.Length;
                        Console.WriteLine($"req: {size}, got: {array.Length}, total: {arrays.Count} for {totalSize}");
                        index = 0;
                    }

                    array[index++] = x;
                    ++count;
                }
            }
            catch
            {
                arrays.ForEach(a => ArrayPool<T>.Shared.Return(a));
                throw;
            }

            Console.WriteLine($"Ticks: {sw.Elapsed.Ticks}");
            return new PooledArray<T>(arrays, count);
        }

        public static PooledArray<U> SelectPooledArray<T, U>(this IReadOnlyList<T> xs, Func<T, U> f)
        {
            var sw = Stopwatch.StartNew();
            var array = ArrayPool<U>.Shared.Rent(xs.Count);
            Console.WriteLine($"req: {xs.Count}, got: {array.Length}");
            try
            {
                var i = 0;
                foreach (var x in xs)
                {
                    array[i++] = f(x);
                }
            }
            catch
            {
                ArrayPool<U>.Shared.Return(array);
                throw;
            }

            Console.WriteLine($"Ticks: {sw.Elapsed.Ticks}");
            return new PooledArray<U>(new List<U[]> { array }, xs.Count);
        }
    }

    public sealed class PooledArray<T> : IDisposable, IReadOnlyList<T>
    {
        private readonly List<T[]> arrays;
        public PooledArray(List<T[]> arrays, int length)
        {
            this.arrays = arrays;
            this.Count = length;
        }

        public T this[int index]
        {
            get
            {
                if (index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                var i = 0;
                while (index >= arrays[i].Length)
                {
                    index -= arrays[i++].Length;
                }

                return arrays[i][index];
            }
        }

        public int Count { get; }

        public void Dispose() => arrays.ForEach(a => ArrayPool<T>.Shared.Return(a));

        public IEnumerator<T> GetEnumerator() => this.arrays.Count == 1 ? (IEnumerator<T>)new Enumerator1(this.arrays[0], this.Count) : new Enumerator(this.arrays, this.Count);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        sealed class Enumerator : IEnumerator<T>
        {
            private int i = 0;
            private int j = -1;
            private int z = -1;
            private readonly List<T[]> arrays;
            private readonly int length;
            public Enumerator(List<T[]> arrays, int length)
            {
                this.arrays = arrays;
                this.length = length;
            }

            public T Current => arrays[i][j];

            object IEnumerator.Current => arrays[i][j];

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (z == length - 1) return false;
                ++z;
                ++j;
                if (j == arrays[i].Length)
                {
                    j = 0;
                    ++i;
                }

                return true;
            }

            public void Reset()
            {
                this.i = 0;
                this.j = -1;
                this.z = -1;
            }
        }

        sealed class Enumerator1 : IEnumerator<T>
        {
            private int i = -1;
            private readonly T[] array;
            private readonly int length;
            public Enumerator1(T[] array, int length)
            {
                this.array = array;
                this.length = length;
            }

            public T Current => array[i];

            object IEnumerator.Current => array[i];

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (i == length - 1) return false;
                ++i;
                return true;
            }

            public void Reset() => this.i = 0;
        }
    }
}