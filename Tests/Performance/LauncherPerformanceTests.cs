using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBench.Util;
using NBench;
using QuantConnect.Configuration;

namespace QuantConnect.Tests.Performance
{
    class LauncherPerformanceTests
    {
        private Counter _counter;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _counter = context.GetCounter("PerfCounter");
        }


        [PerfBenchmark(Description = "Test to measure the runtime of the default program.",
             NumberOfIterations = 3, RunMode = NBench.RunMode.Iterations, TestMode = TestMode.Measurement)]
        [CounterMeasurement("PerfCounter")]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Benchmark()
        {
            var thread = new Thread(() => Lean.Launcher.Program.Main(new string[] {}));
            thread.Start();

            thread.Join();

            _counter.Increment();
        }


        [PerfCleanup]
        public void Cleanup()
        {
            // does nothing
        }
    }
}
