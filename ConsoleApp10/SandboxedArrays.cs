using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PooledArrays;

namespace SandboxedArrays
{
    public sealed class SandboxedArray<T> : IReadOnlyList<T>
    {
        private readonly PooledArray<T> array;
        readonly ConcurrentBag<IDisposable> pooledArrays;
        public SandboxedArray(PooledArray<T> array, ConcurrentBag<IDisposable> pooledArrays)
        {
            this.array = array;
            this.pooledArrays = pooledArrays;
            pooledArrays.Add(array);
        }

        public T this[int index] => array[index];

        public int Count => array.Count;

        public IEnumerator<T> GetEnumerator() => array.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => array.GetEnumerator();

        public SandboxedArray<U> SelectSandboxedArray<U>(Func<T, U> f)
        {
            var array = this.array.SelectPooledArray(f);
            return new SandboxedArray<U>(array, pooledArrays);
        }
    }

    public sealed class LinqSandbox : IDisposable
    {
        readonly ConcurrentBag<IDisposable> pooledArrays = new ConcurrentBag<IDisposable>();

        public SandboxedArray<T> ToSandboxedArray<T>(IEnumerable<T> xs)
        {
            var array = xs.ToPooledArray();
            return new SandboxedArray<T>(array, pooledArrays);
        }

        public void Dispose()
        {
            foreach (var array in pooledArrays)
            {
                array.Dispose();
            }
        }
    }

    public static class LinqSandboxExtensions
    {
        public static SandboxedArray<T> ToSandboxedArray<T>(this IEnumerable<T> xs, LinqSandbox sb) => sb.ToSandboxedArray(xs);
    }

}
