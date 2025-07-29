using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;

namespace UnitTests.CoreSystems.Parsing;

[TestFixture]
public class StringExtensionsTests
{
   [TestCase("\"hello\"", ExpectedResult = "hello")]
   [TestCase("\"quoted string\"", ExpectedResult = "quoted string")]
   [TestCase("\"a\"", ExpectedResult = "a")]
   [TestCase("\"\"", ExpectedResult = "")]
   [TestCase("not quoted", ExpectedResult = "not quoted")]
   [TestCase("  trimmed  ", ExpectedResult = "trimmed")]
   public string TrimQuotes_RemovesSurroundingQuotesIfPresent(string input)
   {
      return input.TrimQuotes();
   }

   [Test]
   public void TrimQuotes_EmptyString_ReturnsEmpty()
   {
      Assert.That(string.Empty.TrimQuotes(), Is.EqualTo(string.Empty));
   }
}