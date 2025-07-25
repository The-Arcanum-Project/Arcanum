using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;

namespace UnitTests;

public class MockSearchable(float relevance = 1.0f, params string[] terms) : ISearchable
{
   public string GetNamespace => string.Empty;
   public string ResultName { get; }
   public List<string> SearchTerms { get; set; } = terms.ToList();

   public void OnSearchSelected()
   {
   }

   public float GetRelevanceScore(string query) => relevance;
}

[TestFixture]
public class QueastorTests
{
   [Test]
   public void AddToIndex_And_ExactSearch_Works()
   {
      var queastor = new Queastor();
      var obj = new MockSearchable(terms:"Button");

      queastor.AddToIndex(obj);

      var results = queastor.Search("Button");
      Assert.That(results, Does.Contain(obj));
   }

   [Test]
   public void FuzzySearch_FindsCloseMatch()
   {
      var queastor = new Queastor();
      var obj = new MockSearchable(terms:"Renderer");

      queastor.AddToIndex(obj);

      var results = queastor.Search("Rendere", 1);
      Assert.That(results, Does.Contain(obj));
   }

   [Test]
   public void NoFalsePositives_OnSearch()
   {
      var queastor = new Queastor();
      var obj1 = new MockSearchable(terms:"Parser");
      var obj2 = new MockSearchable(terms:"Button");

      queastor.AddToIndex(obj1);
      queastor.AddToIndex(obj2);

      var results = queastor.Search("Render");
      Assert.That(results, Is.Empty);
   }

   [Test]
   public void MultipleTerms_MappedCorrectly()
   {
      var queastor = new Queastor();
      var obj = new MockSearchable(1f, "Button", "Control", "Widget");

      queastor.AddToIndex(obj);

      Assert.That(queastor.Search("Button"), Does.Contain(obj));
      Assert.That(queastor.Search("Control"), Does.Contain(obj));
      Assert.That(queastor.Search("Widget"), Does.Contain(obj));
   }

   [Test]
   public void LevenshteinDistance_IsCorrect()
   {
      Assert.That(Queastor.LevenshteinDistance("test", "tast"), Is.EqualTo(1));
      Assert.That(Queastor.LevenshteinDistance("taste", "tast"), Is.EqualTo(1));
      Assert.That(Queastor.LevenshteinDistance("toast", "tast"), Is.EqualTo(1));
   }

   [Test]
   public void RemoveFromIndex_RemovesItemCompletely()
   {
      var queastor = new Queastor();
      var item = new MockSearchable(1f, "Alpha", "Beta");
      queastor.AddToIndex(item);

      queastor.RemoveFromIndex(item);

      var results = queastor.Search("Alpha");
      Assert.IsEmpty(results);

      results = queastor.Search("Beta");
      Assert.IsEmpty(results);
   }

   [Test]
   public void RemoveFromIndex_DoesNothingIfItemNotPresent()
   {
      var queastor = new Queastor();
      var item = new MockSearchable(terms:"Alpha");
      queastor.RemoveFromIndex(item); // Should not throw
   }

   [Test]
   public void ModifyInIndex_UpdatesTermsCorrectly_AddsNewTerms()
   {
      var queastor = new Queastor();
      var item = new MockSearchable(terms:"Alpha");
      queastor.AddToIndex(item);

      item.SearchTerms.Clear();
      item.SearchTerms.Add("Beta");
      item.SearchTerms.Add("Gamma");
      queastor.ModifyInIndex(item, ["Alpha"]);

      Assert.IsEmpty(queastor.Search("Alpha"));
      var results = queastor.Search("Beta");
      Assert.That(results, Does.Contain(item));

      results = queastor.Search("Gamma");
      Assert.That(results, Does.Contain(item));
   }

   [Test]
   public void ModifyInIndex_UpdatesTermsCorrectly_RemovesObsoleteTerms()
   {
      var queastor = new Queastor();
      var item = new MockSearchable(1f, "Alpha", "Beta");
      queastor.AddToIndex(item);

      item.SearchTerms.Clear();
      item.SearchTerms.Add("Beta");
      item.SearchTerms.Add("Gamma");
      queastor.ModifyInIndex(item, ["Alpha", "Beta"]);

      Assert.IsEmpty(queastor.Search("Alpha"));

      var results = queastor.Search("Beta");
      Assert.That(results, Does.Contain(item));

      results = queastor.Search("Gamma");
      Assert.That(results, Does.Contain(item));
   }

   [Test]
   public void ModifyInIndex_HandlesNoChangesGracefully()
   {
      var queastor = new Queastor();
      var item = new MockSearchable(1f, "Alpha", "Beta");
      queastor.AddToIndex(item);

      queastor.ModifyInIndex(item, ["Alpha", "Beta"]);

      var results = queastor.Search("Alpha");
      Assert.That(results, Does.Contain(item));

      results = queastor.Search("Beta");
      Assert.That(results, Does.Contain(item));
   }
   
   [Test]
    public void SortSearchResults_SortsDescendingByDefault()
    {
       var queastor = new Queastor();
        var item1 = new MockSearchable(0.5f, "apple");
        var item2 = new MockSearchable(0.8f, "banana");
        var item3 = new MockSearchable(0.3f, "cherry");

        var list = new List<ISearchable> { item1, item2, item3 };
        var sorted = queastor.SortSearchResults(list, "ap");

        Assert.That(sorted[0].Item2, Is.EqualTo(item2));
        Assert.That(sorted[1].Item2, Is.EqualTo(item1));
        Assert.That(sorted[2].Item2, Is.EqualTo(item3));
    }

    [Test]
    public void SortSearchResults_SortsAscendingWhenRequested()
    {
       var queastor = new Queastor();
        var item1 = new MockSearchable(0.5f, "apple");
        var item2 = new MockSearchable(0.8f, "banana");
        var item3 = new MockSearchable(0.3f, "cherry");

        var list = new List<ISearchable> { item1, item2, item3 };
        var sorted = queastor.SortSearchResults(list, "ap", true);

        Assert.That(sorted[0].Item2, Is.EqualTo(item3));
        Assert.That(sorted[1].Item2, Is.EqualTo(item1));
        Assert.That(sorted[2].Item2, Is.EqualTo(item2));
    }

    [Test]
    public void SortSearchResults_ReturnsEmptyListForEmptyInput()
    {
       var queastor = new Queastor();
        var sorted = queastor.SortSearchResults([], "test");
        Assert.IsEmpty(sorted);
    }

    [Test]
    public void SortSearchResults_ReturnsEmptyListForNullQuery()
    {
        var queastor = new Queastor();
        var item = new MockSearchable(1f, "test");
        
        var sorted = queastor.SortSearchResults([item], null!);
        Assert.IsEmpty(sorted);
    }

    [Test]
    public void GetClosestMatch_ReturnsClosestTerm()
    {
       var queastor = new Queastor();
        var terms = new List<string> { "apple", "apply", "ape" };
        var closest = queastor.GetClosestMatch("appl", terms);
        Assert.That(closest, Is.EqualTo("apple"));
    }

    [Test]
    public void GetClosestMatch_ReturnsEmptyForNoTerms()
    {
       var queastor = new Queastor();
        var closest = queastor.GetClosestMatch("test", []);
        Assert.That(closest, Is.EqualTo(string.Empty));
    }
}