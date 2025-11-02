using BenchmarkDotNet.Attributes;

// ReSharper disable NotAccessedVariable

namespace Benchmarking;

public interface IElement
{
   int Index { get; }
}

public class Block : IElement
{
   public int Index { get; set; }
}

public class Content : IElement
{
   public int Index { get; set; }
}

[MemoryDiagnoser]
public class MergeBenchmark
{
   private List<Block> _blocks = null!;
   private List<Content> _contents = null!;
   private List<IElement> _output = null!;

   [Params(10, 1000, 100_000)]
   public int Count;

   [GlobalSetup]
   public void Setup()
   {
      _blocks = Enumerable.Range(0, Count)
                          .Where(i => i % 2 == 0)
                          .Select(i => new Block { Index = i })
                          .ToList();
      _contents = Enumerable.Range(0, Count)
                            .Where(i => i % 2 != 0)
                            .Select(i => new Content { Index = i })
                            .ToList();
      _output = new(_blocks.Count + _contents.Count);
   }

   [Benchmark]
   public void YieldIterate()
   {
      var sum = 0;
      foreach (var element in MergeBlocksAndContent(_blocks, _contents))
         sum += element.Index;
   }

   [Benchmark]
   public void AsListIterate()
   {
      _output.Clear();
      var sum = 0;
      MergeInto(_blocks, _contents, _output);
      foreach (var element in _output)
         sum += element.Index;
   }

   [Benchmark]
   public List<IElement> MergeBlocksAndContent_Yield()
   {
      return MergeBlocksAndContent(_blocks, _contents).ToList();
   }

   [Benchmark]
   public void MergeInto_OutputList()
   {
      _output.Clear();
      MergeInto(_blocks, _contents, _output);
   }

   public static IEnumerable<IElement> MergeBlocksAndContent(List<Block> blocks, List<Content> contents)
   {
      var ib = 0;
      var ic = 0;
      var cb = blocks.Count;
      var cc = contents.Count;

      while (ib < cb && ic < cc)
      {
         var b = blocks[ib];
         var c = contents[ic];

         if (b.Index < c.Index)
         {
            yield return b;

            ib++;
         }
         else
         {
            yield return c;

            ic++;
         }
      }

      while (ib < cb)
         yield return blocks[ib++];

      while (ic < cc)
         yield return contents[ic++];
   }

   public static void MergeInto(List<Block> blocks, List<Content> contents, List<IElement> output)
   {
      var ib = 0;
      var ic = 0;
      var cb = blocks.Count;
      var cc = contents.Count;

      while (ib < cb && ic < cc)
      {
         var b = blocks[ib];
         var c = contents[ic];

         if (b.Index < c.Index)
         {
            output.Add(b);
            ib++;
         }
         else
         {
            output.Add(c);
            ic++;
         }
      }

      while (ib < cb)
         output.Add(blocks[ib++]);

      while (ic < cc)
         output.Add(contents[ic++]);
   }
}