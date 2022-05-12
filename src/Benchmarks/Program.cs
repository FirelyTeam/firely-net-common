using BenchmarkDotNet.Running;

namespace Firely.Sdk.Benchmarks.Common
{
    public class Program
    {
        public static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<SerializationBenchmarks>();
        }
    }
}
