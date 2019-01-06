using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace OpenSage.Benchmarks.DataStructures
{
    public class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<QuadtreeInsert>();
            BenchmarkRunner.Run<QuadtreeUpdate>();
            BenchmarkRunner.Run<QuadtreeQuery>();
        }
    }
}
