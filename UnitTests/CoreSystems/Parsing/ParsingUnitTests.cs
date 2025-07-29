using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;

// Use the NUnit framework namespace

namespace UnitTests.CoreSystems.Parsing;

[TestFixture] // NUnit attribute to mark a class with tests
public class ElementParserTests
{
   private const string TEST_PATH = "test.txt";
   private TextWriter _originalConsoleOut;

   // Use SetUp and TearDown to manage console output redirection for error tests
   [SetUp]
   public void SetUp()
   {
      _originalConsoleOut = Console.Out;
   }

   [TearDown]
   public void TearDown()
   {
      Console.SetOut(_originalConsoleOut);
   }

   [Test] // NUnit attribute for a single test case
   public void GetElements_EmptyInput_ReturnsEmptyLists()
   {
      // Arrange
      var input = "";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Is.Empty);
   }

   [Test]
   public void GetElements_WhitespaceInput_ReturnsEmptyLists()
   {
      // Arrange
      var input = "  \n\t  \n ";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Is.Empty);
   }

   [Test]
   public void GetElements_TopLevelContentOnly_ParsesCorrectly()
   {
      // Arrange
      var input = "key1 = value1\nkey2 = \"value2\"";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Has.Count.EqualTo(1));
      Assert.That(contents[0].Value.Trim(), Is.EqualTo("key1 = value1\nkey2 = \"value2\""));
   }

   [Test]
   public void GetElements_SingleEmptyBlock_ParsesCorrectly()
   {
      // Arrange
      var input = "my_block = {}";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Is.Empty);
      Assert.That(blocks, Has.Count.EqualTo(1));

      var block = blocks[0];
      Assert.That(block.Name, Is.EqualTo("my_block"));
      Assert.That(block.StartLine, Is.EqualTo(0));
      Assert.That(block.SubBlocks, Is.Empty);
      Assert.That(block.ContentElements, Is.Empty);
   }

   [Test]
   public void GetElements_BlockWithContent_ParsesCorrectly()
   {
      // Arrange
      var input = "data_block = {\n\tkey = \"value\"\n\tkey2 = 123\n}";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Is.Empty);
      Assert.That(blocks, Has.Count.EqualTo(1));

      var block = blocks[0];
      Assert.That(block.Name, Is.EqualTo("data_block"));
      Assert.That(block.SubBlocks, Is.Empty);
      Assert.That(block.ContentElements, Has.Count.EqualTo(1));
      Assert.That(block.ContentElements[0].Value.Trim(), Is.EqualTo("key = \"value\"\nkey2 = 123"));
   }

   [Test]
   public void GetElements_NestedBlocks_ParsesCorrectly()
   {
      // Arrange
      var input = "outer = {\n\tinner = {\n\t\tdeep_key = deep_value\n\t}\n}";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Is.Empty);
      Assert.That(blocks, Has.Count.EqualTo(1));

      var outer = blocks[0];
      Assert.That(outer.Name, Is.EqualTo("outer"));
      Assert.That(outer.ContentElements, Is.Empty);
      Assert.That(outer.SubBlocks, Has.Count.EqualTo(1));

      var inner = outer.SubBlocks[0];
      Assert.That(inner.Name, Is.EqualTo("inner"));
      Assert.That(inner.SubBlocks, Is.Empty);
      Assert.That(inner.ContentElements, Has.Count.EqualTo(1));
      Assert.That(inner.ContentElements[0].Value.Trim(), Is.EqualTo("deep_key = deep_value"));
   }

   [Test]
   public void GetElements_Comments_AreIgnored()
   {
      // Arrange
      var input = "# Top level comment\nkey = value # Inline comment\nblock = { # Another comment\n\tval = 1\n}";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Has.Count.EqualTo(1));
      Assert.That(contents[0].Value.Trim(), Is.EqualTo("key = value"));

      Assert.That(blocks, Has.Count.EqualTo(1));
      var block = blocks[0];
      Assert.That(block.Name, Is.EqualTo("block"));
      Assert.That(block.ContentElements, Has.Count.EqualTo(1));
      Assert.That(block.ContentElements[0].Value.Trim(), Is.EqualTo("val = 1"));
   }

   [Test]
   public void GetElements_HeinousFormatting_ParsesBlockName()
   {
      // Arrange
      var input = "this_is_a_block_name\n{\n\tkey = value\n}";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Is.Empty);
      Assert.That(blocks, Has.Count.EqualTo(1));
      var block = blocks[0];
      Assert.That(block.Name, Is.EqualTo("this_is_a_block_name"));
      Assert.That(block.ContentElements, Has.Count.EqualTo(1));
      Assert.That(block.ContentElements[0].Value.Trim(), Is.EqualTo("key = value"));
   }

   [Test]
   public void GetElements_EscapedQuotesAndCharacters_AreHandled()
   {
      // Arrange
      var input = "key = \"a value with \\\"quotes\\\" and a backslash \\\\ in it\"";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Has.Count.EqualTo(1));
      Assert.That(contents[0].Value.Trim(),
                  Is.EqualTo("key = \"a value with \\\"quotes\\\" and a backslash \\\\ in it\""));
   }

   [Test]
   public void GetElements_SpecialCharsInsideQuotes_AreTreatedAsLiterals()
   {
      // Arrange
      var input = "key = \"value with { # and } inside\"";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Has.Count.EqualTo(1));
      Assert.That(contents[0].Value.Trim(), Is.EqualTo("key = \"value with { # and } inside\""));
   }

   [Test]
   public void GetElements_MixOfContentAndBlocks_ParsesCorrectly()
   {
      // Arrange
      var input = "top1 = 1\n\nblock1 = {\n\tinner = val\n}\n\ntop2 = 2";

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(contents, Has.Count.EqualTo(2));
      Assert.That(contents[0].Value.Trim(), Is.EqualTo("top1 = 1"));
      Assert.That(contents[1].Value.Trim(), Is.EqualTo("top2 = 2"));

      Assert.That(blocks, Has.Count.EqualTo(1));
      Assert.That(blocks[0].Name, Is.EqualTo("block1"));
   }

   // --- Error Condition Tests ---

   [Test]
   public void GetElements_UnmatchedOpeningBrace_ReturnsEmptyAndLogsError()
   {
      // Arrange
      var input = "my_block = {";
      using var consoleOutput = new StringWriter();
      Console.SetOut(consoleOutput);

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Is.Empty);
      var output = consoleOutput.ToString();
      Assert.That(output, Does.Contain("Unmatched opening brace"));
      Assert.That(output, Does.Contain(TEST_PATH));
   }

   [Test]
   public void GetElements_UnmatchedClosingBrace_ReturnsEmptyAndLogsError()
   {
      // Arrange
      var input = "key = value\n}";
      using var consoleOutput = new StringWriter();
      Console.SetOut(consoleOutput);

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Is.Empty);
      var output = consoleOutput.ToString();
      Assert.That(output, Does.Contain("Unmatched closing brace"));
      Assert.That(output, Does.Contain(TEST_PATH));
   }

   [Test]
   public void GetElements_MissingBlockName_ReturnsEmptyAndLogsError()
   {
      // Arrange
      var input = "= {}";
      using var consoleOutput = new StringWriter();
      Console.SetOut(consoleOutput);

      // Act
      var (blocks, contents) = ElementParser.GetElements(TEST_PATH, input);

      // Assert
      Assert.That(blocks, Is.Empty);
      Assert.That(contents, Is.Empty);
      var output = consoleOutput.ToString();
      Assert.That(output, Does.Contain("Block name cannot be empty"));
   }
}