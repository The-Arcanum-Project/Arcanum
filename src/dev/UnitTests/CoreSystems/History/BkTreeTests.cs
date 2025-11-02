using Arcanum.Core.CoreSystems.Queastor;

namespace UnitTests.CoreSystems.History;

[TestFixture]
public class BkTreeTests
{
   [Test]
   public void AddAndExactSearch_Works()
   {
      var tree = new BkTree();
      tree.Add("test");
      tree.Add("toast");

      var results = tree.Search("test", 0);
      Assert.That(results, Does.Contain("test"));
      Assert.That(results, Does.Not.Contain("toast"));
   }

   [Test]
   public void FuzzySearch_FindsCloseMatches()
   {
      var tree = new BkTree();
      tree.Add("test");
      tree.Add("toast");
      tree.Add("taste");

      var results = tree.Search("tast", 1);

      Assert.That(results, Does.Contain("taste"));
      Assert.That(results, Does.Contain("test"));
      Assert.That(results, Does.Not.Contain("tooast"));
   }

   [Test]
   public void Search_EmptyTree_ReturnsEmpty()
   {
      var tree = new BkTree();
      var results = tree.Search("anything", 2);
      Assert.That(results, Is.Empty);
   }

   [Test]
   public void Add_DuplicateTerms_IgnoredOrHandled()
   {
      var tree = new BkTree();
      tree.Add("duplicate");
      tree.Add("duplicate");
      tree.Add("duplicate");

      var results = tree.Search("duplicate", 0);
      Assert.That(results.Count, Is.EqualTo(1));
   }

   [Test]
   public void Search_NoMatchWithinDistance_ReturnsEmpty()
   {
      var tree = new BkTree();
      tree.Add("apple");
      tree.Add("banana");
      tree.Add("cherry");

      var results = tree.Search("xyz", 1);
      Assert.That(results, Is.Empty);
   }

   [Test]
   public void Search_CaseInsensitive_MatchesRegardlessOfCase()
   {
      var tree = new BkTree();
      tree.Add("TestCase");
      tree.Add("tOAst");

      var results = tree.Search("testcase".ToLowerInvariant(), 1);
      Assert.That(results, Does.Contain("TestCase".ToLowerInvariant()));
   }

   [Test]
   public void Search_MaxDistanceZero_ReturnsExactMatchesOnly()
   {
      var tree = new BkTree();
      tree.Add("exact");
      tree.Add("exalt");

      var results = tree.Search("exact", 0);
      Assert.That(results, Does.Contain("exact"));
      Assert.That(results, Does.Not.Contain("exalt"));
   }
}