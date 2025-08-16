using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarking;



public class Program
{
   public static void Main(string[] args)
   {
      BenchmarkRunner.Run<IterationBenchmark>();
   }
}