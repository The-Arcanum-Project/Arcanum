using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;

namespace UnitTests.CoreSystems.Parsing;

[TestFixture]
public class IElementMergeIntoTests
{
   [Test]
   public void MergeInto_InterleavesCorrectly()
   {
      var blocks = new List<Block> { new("block1", 1, 0), new("block2", 2, 3) };
      var contents = new List<Content> { new("val1", 1, 1), new("val2", 2, 2) };

      var result = new List<IElement>();
      IElement.MergeInto(blocks, contents, result);

      Assert.That(result.Select(e => e.Index), Is.EqualTo(new[] { 0, 1, 2, 3 }));
   }

   [Test]
   public void MergeInto_BlocksEmpty_AddsOnlyContents()
   {
      var blocks = new List<Block>();
      var contents = new List<Content> { new("val", 1, 0) };

      var result = new List<IElement>();
      IElement.MergeInto(blocks, contents, result);

      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0], Is.TypeOf<Content>());
   }

   [Test]
   public void MergeInto_ContentsEmpty_AddsOnlyBlocks()
   {
      var blocks = new List<Block> { new("b", 1, 0) };
      var contents = new List<Content>();

      var result = new List<IElement>();
      IElement.MergeInto(blocks, contents, result);

      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0], Is.TypeOf<Block>());
   }

   [Test]
   public void MergeInto_DuplicateIndices_OrderPreserved()
   {
      var blocks = new List<Block> { new("b", 1, 1) };
      var contents = new List<Content> { new("c", 1, 1) };

      var result = new List<IElement>();
      IElement.MergeInto(blocks, contents, result);

      Assert.That(result.Count, Is.EqualTo(2));
      Assert.That(result[0], Is.TypeOf<Content>()); // Content inserted first due to else
      Assert.That(result[1], Is.TypeOf<Block>());
   }

   [Test]
   public void MergeInto_NullInputs_Throws()
   {
      Assert.Throws<ArgumentNullException>(() => { IElement.MergeInto(null!, [], []); });

      Assert.Throws<ArgumentNullException>(() => { IElement.MergeInto([], null!, []); });

      Assert.Throws<ArgumentNullException>(() => { IElement.MergeInto([], [], null!); });
   }

   [Test]
   public void MergeBlocksAndContent_InterleavesCorrectly()
   {
      var blocks = new List<Block> { new("b1", 1, 0), new("b2", 1, 4) };
      var contents = new List<Content> { new("c1", 1, 1), new("c2", 1, 3) };

      var result = IElement.MergeBlocksAndContent(blocks, contents).ToList();

      Assert.That(result.Select(e => e.Index), Is.EqualTo(new[] { 0, 1, 3, 4 }));
   }

   [Test]
   public void MergeBlocksAndContent_EmptyInputs_ReturnsEmpty()
   {
      var result = IElement.MergeBlocksAndContent([], []).ToList();
      Assert.That(result, Is.Empty);
   }

   [Test]
   public void MergeBlocksAndContent_OnlyBlocks_ReturnsBlocks()
   {
      var blocks = new List<Block> { new("b", 1, 2) };
      var result = IElement.MergeBlocksAndContent(blocks, []).ToList();

      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0], Is.TypeOf<Block>());
   }

   [Test]
   public void MergeBlocksAndContent_OnlyContents_ReturnsContents()
   {
      var contents = new List<Content> { new("c", 1, 2) };
      var result = IElement.MergeBlocksAndContent([], contents).ToList();

      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0], Is.TypeOf<Content>());
   }

   [Test]
   public void MergeBlocksAndContent_DuplicateIndices_ReturnsAll()
   {
      var blocks = new List<Block> { new("b", 1, 1) };
      var contents = new List<Content> { new("c", 1, 1) };

      var result = IElement.MergeBlocksAndContent(blocks, contents).ToList();

      Assert.That(result.Count, Is.EqualTo(2));
      Assert.That(result.Any(e => e is Block));
      Assert.That(result.Any(e => e is Content));
   }

   [Test]
   public void MergeBlocksAndContent_NullInputs_Throws()
   {
      Assert.Throws<ArgumentNullException>(() => { _ = IElement.MergeBlocksAndContent(null!, []).ToList(); });

      Assert.Throws<ArgumentNullException>(() => { _ = IElement.MergeBlocksAndContent([], null!).ToList(); });
   }
}