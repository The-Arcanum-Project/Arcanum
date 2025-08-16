namespace Benchmarking;

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class IterationBenchmark
{
   private Dictionary<int, int> _dict = [];
   private HashSet<int> _set = [];

   private const int COUNT = 30_000;

   [GlobalSetup]
   public void Setup()
   {
      _dict = new(COUNT);
      _set = new(COUNT);

      for (var i = 0; i < COUNT; i++)
      {
         if (i % 2 == 0)
            _dict[i] = i;
         _set.Add(i);
      }
   }

   [Benchmark]
   public int IterateDictionaryValues()
   {
      var sum = 0;
      foreach (var v in _dict.Values)
         sum += v;
      return sum;
   }

   [Benchmark]
   public int IterateHashSet()
   {
      var sum = 0;
      foreach (var v in _set)
         sum += v;
      return sum;
   }
}