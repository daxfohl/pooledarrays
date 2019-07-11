using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using PooledArrays;

namespace ConsoleApp10
{
	class Program
	{
        static void Main(string[] args)
        {
            using (var sb = new LinqSandbox())
            {
                var bs = Enumerable.Range(0, 10000000);
                var cs = sb.SelectArray(bs, i => i + 1);
                var ds = sb.SelectArray(cs, i => i + 1);
                var es = sb.SelectArray(ds, i => i + 1);
                var fs = sb.SelectArray(es, i => i + 1);
                var gs = sb.SelectArray(fs, i => i + 1);
                var hs = sb.SelectArray(gs, i => i + 1);
                var ks = sb.SelectArray(hs, i => i + 1);
                Console.WriteLine(es.Max());
                Console.WriteLine(es.Max());
                Console.WriteLine(es.Max());
            }

            Console.ReadKey();
        }
	}

    class LinqSandbox : IDisposable
    {
        List<IDisposable> pooledArrays = new List<IDisposable>();

        public PooledArray<U> SelectArray<T, U>(IEnumerable<T> xs, Func<T, U> f)
        {
            var array = xs.SelectArray(f);
            pooledArrays.Add(array);
            return array;
        }

        public void Dispose()
        {
            foreach (var array in pooledArrays)
            {
                array.Dispose();
            }
        }
    }

}
