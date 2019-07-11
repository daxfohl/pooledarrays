using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace PooledArrays
{
    public static class PooledArrayExtensions
    {
        public static PooledArray<U> SelectPooledArray<T, U>(this IEnumerable<T> xs, Func<T, U> f)
        {
            if (xs is IReadOnlyList<T>)
            {
                return SelectPooledArray((IReadOnlyList<T>)xs, f);
            }

            var array = ArrayPool<U>.Shared.Rent(0);
            Console.WriteLine($"req: 0, got: {array.Length}");
            int i = 0;
            try
            {
                foreach (var x in xs)
                {
                    if (i >= array.Length)
                    {
                        var size = i == 0 ? 1 : i * 2;
                        var newArray = ArrayPool<U>.Shared.Rent(size);
                        Console.WriteLine($"req: {size}, got: {newArray.Length}");
                        Array.Copy(array, 0, newArray, 0, i);
                        ArrayPool<U>.Shared.Return(array);
                        array = newArray;
                    }

                    array[i] = f(x);
                    ++i;
                }
            }
            catch
            {
                ArrayPool<U>.Shared.Return(array);
                throw;
            }

            return new PooledArray<U>(array, i);
        }

        public static PooledArray<U> SelectPooledArray<T, U>(this IReadOnlyList<T> xs, Func<T, U> f)
        {
            var array = ArrayPool<U>.Shared.Rent(xs.Count);
            Console.WriteLine($"req: {xs.Count}, got: {array.Length}");
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

            return new PooledArray<U>(array, xs.Count);
        }
    }

    public class PooledArray<T> : IDisposable, IReadOnlyList<T>
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

        class Enumerator : IEnumerator<T>
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