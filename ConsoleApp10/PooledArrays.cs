using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PooledArrays
{
    public static class PooledArrayExtensions
    {
        public static PooledArray<T> ToPooledArray<T>(this IEnumerable<T> xs)
        {
            var sw = Stopwatch.StartNew();
            var array = ArrayPool<T>.Shared.Rent(0);
            var arrays = ArrayPool<T[]>.Shared.Rent(32);
            var totalSize = array.Length;
            var index = 0;
            Debug.WriteLine($"req: 0, got: {array.Length}, total: 1 for {totalSize}");
            int count = 0;
            var arraysIndex = 0;
            arrays[arraysIndex++] = array;
            try
            {
                foreach (var x in xs)
                {
                    if (index >= array.Length)
                    {
                        var size = count == 0 ? 1 : count;
                        array = ArrayPool<T>.Shared.Rent(size);
                        arrays[arraysIndex++] = array;
                        totalSize += array.Length;
                        Debug.WriteLine($"req: {size}, got: {array.Length}, total: {arrays.Length} for {totalSize}");
                        index = 0;
                    }

                    array[index++] = x;
                    ++count;
                }

                var start = 0;
                var final = ArrayPool<T>.Shared.Rent(count);
                for (var i = 0; i < arraysIndex; ++i)
                {
                    var arr = arrays[i];
                    Array.Copy(arr, 0, final, start, Math.Min(arr.Length, count - start));
                    start += arr.Length;
                }

                Debug.WriteLine($"Ticks: {sw.Elapsed.Ticks}");
                return new PooledArray<T>(final, count);
            }
            finally
            {
                for (var i = 0; i < arraysIndex; ++i) ArrayPool<T>.Shared.Return(arrays[i]);
                ArrayPool<T[]>.Shared.Return(arrays);
            }
        }


        public static PooledArray<U> SelectPooledArray<T, U>(this IEnumerable<T> xs, Func<T, U> f)
        {
            if (xs is IReadOnlyList<T>)
            {
                return SelectPooledArray((IReadOnlyList<T>)xs, f);
            }

            return xs.ToPooledArray().SelectPooledArray(f);
        }

        public static PooledArray<U> SelectPooledArray<T, U>(this IReadOnlyList<T> xs, Func<T, U> f)
        {
            var sw = Stopwatch.StartNew();
            var array = ArrayPool<U>.Shared.Rent(xs.Count);
            //Debug.WriteLine($"req: {xs.Count}, got: {array.Length}");
            try
            {
                for (var i = 0; i < xs.Count; ++i)
                {
                    array[i] = f(xs[i]);
                }
            }
            catch
            {
                ArrayPool<U>.Shared.Return(array);
                throw;
            }

            Debug.WriteLine($"Ticks: {sw.Elapsed.Ticks}");
            return new PooledArray<U>(array, xs.Count);
        }
    }

    public sealed class PooledArray<T> : IDisposable, IReadOnlyList<T>
    {
        private readonly T[] array;
        public PooledArray(T[] array, int length)
        {
            this.array = array;
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

                return array[index];
            }
        }

        public int Count { get; }

        public void Dispose() => ArrayPool<T>.Shared.Return(array);

        public IEnumerator<T> GetEnumerator() => new Enumerator(this.array, this.Count);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(array, this.Count);

        sealed class Enumerator : IEnumerator<T>
        {
            private int i = -1;
            private readonly T[] array;
            private readonly int length;
            public Enumerator(T[] array, int length)
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