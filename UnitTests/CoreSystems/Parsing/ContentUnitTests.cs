using System.Text;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace UnitTests.CoreSystems.Parsing;

[TestFixture]
public class ContentTests
{
   private Content _content;

   [SetUp]
   public void SetUp()
   {
      _content = new("key=value\nfoo=bar", 10, 0);
   }

   [Test]
   public void Constructor_SetsPropertiesCorrectly()
   {
      Assert.That(_content.Value, Is.EqualTo("key=value\nfoo=bar"));
      Assert.That(_content.StartLine, Is.EqualTo(10));
      Assert.That(_content.Index, Is.EqualTo(0));
      Assert.That(_content.IsBlock, Is.False);
   }

   [Test]
   public void GetContent_ReturnsValue()
   {
      Assert.That(_content.GetContent(), Is.EqualTo("key=value\nfoo=bar"));
   }

   [Test]
   public void ToString_ReturnsValue()
   {
      Assert.That(_content.ToString(), Is.EqualTo("key=value\nfoo=bar"));
   }

   [Test]
   public void GetLineEnumerator_ReturnsLinesWithCorrectLineNumbers()
   {
      var result = _content.GetLineEnumerator().ToList();

      Assert.That(result.Count, Is.EqualTo(2));
      Assert.That(result[0], Is.EqualTo(("key=value", 10)));
      Assert.That(result[1], Is.EqualTo(("foo=bar", 11)));
   }

   [Test]
   public void GetLineEnumerator_SkipsEmptyLines()
   {
      var c = new Content("line1\n\nline2", 5, 0);
      var result = c.GetLineEnumerator().ToList();

      Assert.That(result, Has.Count.EqualTo(2));
      Assert.That(result[0].Item2, Is.EqualTo(5));
      Assert.That(result[1].Item2, Is.EqualTo(7));
   }

   [Test]
   public void GetStringListEnumerator_SplitsBySpace()
   {
      var c = new Content("a b c\nd e", 3, 0);
      var result = c.GetStringListEnumerator().ToList();

      Assert.That(result.Select(r => r.Item1), Is.EqualTo(new[] { "a", "b", "c", "d", "e" }));
      Assert.That(result[0].Item2, Is.EqualTo(3));
      Assert.That(result[3].Item2, Is.EqualTo(4));
   }

   [Test]
   public void GetLineKvpEnumerator_ParsesKeyValuePairs()
   {
      var result = _content.GetLineKvpEnumerator(PathObj.Empty).ToList();

      Assert.That(result.Count, Is.EqualTo(2));
      Assert.That(result[0].Key, Is.EqualTo("key"));
      Assert.That(result[0].Value, Is.EqualTo("value"));
      Assert.That(result[0].Line, Is.EqualTo(10));
   }

   [Test]
   public void GetLineKvpEnumerator_TrimsQuotesIfEnabled()
   {
      var c = new Content("a=\"123\"", 1, 0);
      var result = c.GetLineKvpEnumerator(PathObj.Empty, trimQuotes: true).First();
      Assert.That(result.Value, Is.EqualTo("123"));
   }

   [Test]
   public void GetLineKvpEnumerator_KeepsQuotesIfDisabled()
   {
      var c = new Content("a=\"123\"", 1, 0);
      var result = c.GetLineKvpEnumerator(PathObj.Empty, trimQuotes: false).First();
      Assert.That(result.Value, Is.EqualTo("\"123\""));
   }

   [Test]
   public void GetLineKvpEnumerator_SkipsMalformedLines()
   {
      var c = new Content("valid=1\ninvalid\nkey=value", 1, 0);
      var result = c.GetLineKvpEnumerator(PathObj.Empty, showError: false).ToList();
      Assert.That(result.Count, Is.EqualTo(2));
   }

   [Test]
   public void GetFormattedString_AppendsFormattedContent()
   {
      var sb = new StringBuilder();
      var result = _content.GetFormattedString(0, ref sb);

      Assert.That(result, Does.Contain("key"));
      Assert.That(result, Does.Contain("value"));
   }

   [Test]
   public void AppendFormattedContent_AppendsToStringBuilder()
   {
      var sb = new StringBuilder("header\n");
      _content.AppendFormattedContent(1, ref sb);

      Assert.That(sb.ToString(), Does.Contain("key"));
      Assert.That(sb.ToString(), Does.StartWith("header\n"));
   }
}