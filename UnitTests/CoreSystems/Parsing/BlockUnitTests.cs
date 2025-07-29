using System.Text;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem;

namespace UnitTests.CoreSystems.Parsing;

[TestFixture]
public class BlockTests
{
   private Block _block;

   [SetUp]
   public void SetUp()
   {
      _block = new("root", 1, 0);
   }

   private static PathObj DummyPath => new("dummy/path.txt");

   [Test]
   public void Constructor_InitializesProperties()
   {
      Assert.That(_block.Name, Is.EqualTo("root"));
      Assert.That(_block.StartLine, Is.EqualTo(1));
      Assert.That(_block.Index, Is.EqualTo(0));
      Assert.That(_block.Count, Is.EqualTo(0));
      Assert.That(_block.IsBlock, Is.True);
   }

   [Test]
   public void GetSubBlocks_WithOnlyBlocksTrue_ThrowsIfContentExists()
   {
      _block.ContentElements.Add(new("value", 1, 0));
      Assert.Throws<ArgumentException>(() => _block.GetSubBlocks(true));
   }

   [Test]
   public void GetContentElements_WithOnlyContentTrue_WarnsIfSubBlocksExist()
   {
      _block.SubBlocks.Add(new("child", 2, 1));
      var result = _block.GetContentElements(true, DummyPath);
      Assert.That(result, Is.Empty);
   }

   [Test]
   public void GetContentLines_ReturnsCorrectLines()
   {
      var content = new Content("key = value", 1, 0);

      _block.ContentElements.Add(content);

      var lines = _block.GetContentLines(DummyPath);

      Assert.That(lines.Count, Is.EqualTo(1));
      Assert.That(lines[0].Key, Is.EqualTo("key"));
      Assert.That(lines[0].Value, Is.EqualTo("value"));
   }

   [Test]
   public void GetContent_ReturnsExpectedString()
   {
      var content = new Content("key = value", 1, 0);
      _block.ContentElements.Add(content);

      var result = _block.GetContent();

      Assert.That(result, Does.Contain("key"));
      Assert.That(result, Does.Contain("value"));
   }

   [Test]
   public void GetFormattedString_AppendsFormattedBlock()
   {
      var sb = new StringBuilder();
      _block.GetFormattedString(0, ref sb);
      Assert.That(sb.ToString(), Does.Contain("{").And.Contain("}"));
   }

   [Test]
   public void GetElements_ReturnsMergedInCorrectOrder()
   {
      var b = new Block("a", 1, 0);
      var c = new Content("key = value", 1, 1);
      _block.SubBlocks.Add(b);
      _block.ContentElements.Add(c);

      var result = _block.GetElements().ToList();

      Assert.That(result[0], Is.EqualTo(b));
      Assert.That(result[1], Is.EqualTo(c));
   }

   [Test]
   public void GetBlockByName_FindsCorrectBlock()
   {
      var child = new Block("child", 2, 0);
      _block.SubBlocks.Add(child);

      var success = _block.GetSubBlockByName("child", out var found);

      Assert.That(success, Is.True);
      Assert.That(found, Is.EqualTo(child));
   }

   [Test]
   public void GetBlockByName_ReturnsFalseOnMissing()
   {
      var success = _block.GetSubBlockByName("missing", out var found);
      Assert.That(success, Is.False);
      Assert.That(found, Is.Null);
   }

   [Test]
   public void GetAllSubBlockByName_FindsMultiple()
   {
      _block.SubBlocks.Add(new("x", 2, 1));
      _block.SubBlocks.Add(new("x", 3, 2));

      var success = _block.GetAllSubBlockByName("x", out var blocks);

      Assert.That(success, Is.True);
      Assert.That(blocks.Count, Is.EqualTo(2));
   }

   [Test]
   public void GetAllSubBlockByName_ReturnsFalseIfNone()
   {
      var success = _block.GetAllSubBlockByName("nope", out var blocks);
      Assert.That(success, Is.False);
      Assert.That(blocks, Is.Empty);
   }
   
   [Test]
   public void ToString_ReturnsName()
   {
      Assert.That(_block.ToString(), Is.EqualTo("root"));
   }
}
