using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FileUploadBufferSize>(
                ManualConfig
                .Create(DefaultConfig.Instance)
                .With(Job.Clr
                    .WithWarmupCount(0)
                    .WithLaunchCount(1)
                    .WithTargetCount(3)));

            Console.WriteLine(summary);
            Console.ReadLine();
        }
    }
}
