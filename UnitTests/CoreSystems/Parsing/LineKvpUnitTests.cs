using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;

namespace UnitTests.CoreSystems.Parsing;

[TestFixture]
public class LineKvpTests
{
   [Test]
   public void Constructor_AssignsPropertiesCorrectly()
   {
      var kvp = new LineKvp<string, string>("key", "value", 3);
      Assert.That(kvp.Key, Is.EqualTo("key"));
      Assert.That(kvp.Value, Is.EqualTo("value"));
      Assert.That(kvp.Line, Is.EqualTo(3));
   }

   [Test]
   public void ToString_ReturnsExpectedFormat()
   {
      var kvp = new LineKvp<string, string>("foo", "bar", 1);
      Assert.That(kvp.ToString(), Is.EqualTo("foo = bar"));
   }

   [Test]
   public void Deconstruct_WorksCorrectly()
   {
      var kvp = new LineKvp<string, string>("key", "val", 4);
      var (k, v, l) = kvp;
      Assert.That(k, Is.EqualTo("key"));
      Assert.That(v, Is.EqualTo("val"));
      Assert.That(l, Is.EqualTo(4));
   }

   [Test]
   public void ImplicitTupleConversion_WorksCorrectly()
   {
      LineKvp<string, string> kvp = ("a", "b", 6);
      Assert.That(kvp.Key, Is.EqualTo("a"));
      Assert.That(kvp.Value, Is.EqualTo("b"));
      Assert.That(kvp.Line, Is.EqualTo(6));
   }

   [Test]
   public void Equals_ReturnsTrue_WhenValuesMatch()
   {
      var a = new LineKvp<string, string>("k", "v", 5);
      var b = new LineKvp<string, string>("k", "v", 5);
      Assert.That(a.Equals(b), Is.True);
   }

   [Test]
   public void Equals_ReturnsFalse_WhenKeyDiffers()
   {
      var a = new LineKvp<string, string>("k1", "v", 1);
      var b = new LineKvp<string, string>("k2", "v", 1);
      Assert.That(a.Equals(b), Is.False);
   }

   [Test]
   public void Equals_ReturnsFalse_WhenValueDiffers()
   {
      var a = new LineKvp<string, string>("k", "v1", 1);
      var b = new LineKvp<string, string>("k", "v2", 1);
      Assert.That(a.Equals(b), Is.False);
   }

   [Test]
   public void Equals_ReturnsFalse_WhenLineDiffers()
   {
      var a = new LineKvp<string, string>("k", "v", 1);
      var b = new LineKvp<string, string>("k", "v", 2);
      Assert.That(a.Equals(b), Is.False);
   }

   [Test]
   public void GetHashCode_DiffersForDifferentLines()
   {
      var a = new LineKvp<string, string>("k", "v", 1);
      var b = new LineKvp<string, string>("k", "v", 2);
      Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
   }

   [Test]
   public void GetHashCode_MatchesForEqualKvp()
   {
      var a = new LineKvp<string, string>("k", "v", 5);
      var b = new LineKvp<string, string>("k", "v", 5);
      Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
   }
}
