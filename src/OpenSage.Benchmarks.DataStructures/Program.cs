using BenchmarkDotNet.Running;

namespace OpenSage.Benchmarks.DataStructures
{
    public class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<BitArray512CheckExisting>();
            BenchmarkRunner.Run<BitArray512SetAllAndCount>();
            BenchmarkRunner.Run<BitArray512SetAllExceptOneAndCount>();
            BenchmarkRunner.Run<BitArray512SetHalfRandomlyAndCount>();
            BenchmarkRunner.Run<QuadtreeInsert>();
            BenchmarkRunner.Run<QuadtreeUpdate>();
            BenchmarkRunner.Run<QuadtreeQuery>();
            BenchmarkRunner.Run<PathfindingQuery>();
        }
    }
}
