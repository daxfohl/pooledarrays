using System;
using System.Linq;
using PooledArrays;
using SandboxedArrays;

namespace ConsoleApp10
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sb = new LinqSandbox())
            {
                var bs = Enumerable.Range(0, 10000000).ToSandboxedArray(sb);
                var cs = bs.SelectSandboxedArray(i => i + 1);
                var ds = cs.SelectSandboxedArray(i => i + 1);
                var es = ds.SelectSandboxedArray(i => i + 1);
                var fs = es.SelectSandboxedArray(i => i + 1);
                var gs = fs.SelectSandboxedArray(i => i + 1);
                var hs = gs.SelectSandboxedArray(i => i + 1);
                var ks = hs.SelectSandboxedArray(i => i + 1);
                Console.WriteLine(es.Max());
                Console.WriteLine(es.Max());
                Console.WriteLine(es.Max());
            }

            Console.ReadKey();
        }
    }
}
