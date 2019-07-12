using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using SandboxedArrays;

namespace ConsoleApp10

{
    class Program
    {
        static void Main(string[] args) => BenchmarkRunner.Run<Pooling>();
    }

    [MemoryDiagnoser]
    [Config(typeof(DontForceGcCollectionsConfig))] // we don't want to interfere with GC, we want to include it's impact
    public class Pooling
    {
        [Params((int)1E+2, // 100 bytes
            (int)1E+3, // 1 000 bytes = 1 KB
            (int)1E+4, // 10 000 bytes = 10 KB
            (int)1E+5, // 100 000 bytes = 100 KB
            (int)1E+6, // 1 000 000 bytes = 1 MB
            (int)1E+7)] // 10 000 000 bytes = 10 MB
        public int SizeInBytes { get; set; }

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