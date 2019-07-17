using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PooledArrays;
using SandboxedArrays;

namespace ConsoleApp10

{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Pooling>();
        }
    }

    [MemoryDiagnoser]
    [Config(typeof(DontForceGcCollectionsConfig))] // we don't want to interfere with GC, we want to include it's impact
    public class Pooling
    {
        [Params((int)1E+6)]
        public int SizeInBytes { get; set; } = 1000;

        [Benchmark]
        public void Sandbox()
        {
            using (var sb = new LinqSandbox())
            {
                var bs = Enumerable.Range(0, SizeInBytes).ToSandboxedArray(sb);
                var cs = bs.SelectSandboxedArray(i => i + 1);
                var ds = cs.SelectSandboxedArray(i => i + 1);
                var es = ds.SelectSandboxedArray(i => i + 1);
                var fs = es.SelectSandboxedArray(i => i + 1);
                var gs = fs.SelectSandboxedArray(i => i + 1);
                var hs = gs.SelectSandboxedArray(i => i + 1);
                var ks = hs.SelectSandboxedArray(i => i + 1);
                var x = es.Max();
                var y = es.Max();
                var z = es.Max();
            }
        }

        [Benchmark]
        public void Pool()
        {
            using var bs = Enumerable.Range(0, SizeInBytes).ToPooledArray();
            using var cs = bs.SelectPooledArray(i => i + 1);
            using var ds = cs.SelectPooledArray(i => i + 1);
            using var es = ds.SelectPooledArray(i => i + 1);
            using var fs = es.SelectPooledArray(i => i + 1);
            using var gs = fs.SelectPooledArray(i => i + 1);
            using var hs = gs.SelectPooledArray(i => i + 1);
            using var ks = hs.SelectPooledArray(i => i + 1);
            var x = es.Max();
            var y = es.Max();
            var z = es.Max();
        }

        [Benchmark]
        public void Regular()
        {
            var bs = Enumerable.Range(0, SizeInBytes).ToArray();
            var cs = bs.Select(i => i + 1).ToArray();
            var ds = cs.Select(i => i + 1).ToArray();
            var es = ds.Select(i => i + 1).ToArray();
            var fs = es.Select(i => i + 1).ToArray();
            var gs = fs.Select(i => i + 1).ToArray();
            var hs = gs.Select(i => i + 1).ToArray();
            var ks = hs.Select(i => i + 1).ToArray();
            var x = es.Max();
            var y = es.Max();
            var z = es.Max();
        }
    }

    public class DontForceGcCollectionsConfig : ManualConfig
    {
        public DontForceGcCollectionsConfig()
        {
            Add(Job.Default
                .With(new GcMode()
                {
                    Force = false // tell BenchmarkDotNet not to force GC collections after every iteration
                }));
        }
    }
}