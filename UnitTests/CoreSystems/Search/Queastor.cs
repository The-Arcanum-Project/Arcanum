using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;

namespace UnitTests.CoreSystems.Search;

public class MockSearchable(float relevance = 1.0f, params string[] terms) : ISearchable
{
   public string GetNamespace => string.Empty;
   public string ResultName { get; } = null!;
   public List<string> SearchTerms { get; set; } = terms.ToList();

   public void OnSearchSelected()
   {
   }

   public float GetRelevanceScore(string query) => relevance;
   public ISearchResult VisualRepresentation { get; } = null!;
   public IQueastorSearchSettings.Category SearchCategory { get; } = IQueastorSearchSettings.Category.All;
}

[TestFixture]
public class QueastorTests
{
   [Test]
   public void AddToIndex_And_ExactSearch_Works()
   {
      var queastor = new Queastor(new ());
      var obj = new MockSearchable(terms:"Button");

      queastor.AddToIndex(obj);

      var results = queastor.Search("Button");
      Assert.That(results, Does.Contain(obj));
   }

   [Test]
   public void FuzzySearch_FindsCloseMatch()
   {
      var queastor = new Queastor(new (){MaxLevinsteinDistance = 1});
      var obj = new MockSearchable(terms:"Renderer");

      queastor.AddToIndex(obj);

      var results = queastor.Search("Rendere");
      Assert.That(results, Does.Contain(obj));
   }

   [Test]
   public void NoFalsePositives_OnSearch()
   {
      var queastor = new Queastor(new ());
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
      var queastor = new Queastor(new ());
      var obj = new MockSearchable(1f, "Button", "Control", "Widget");

      queastor.AddToIndex(obj);

      Assert.That(queastor.Search("Button"), Does.Contain(obj));
      Assert.That(queastor.Search("Control"), Does.Contain(obj));
      Assert.That(queastor.Search("Widget"), Does.Contain(obj));
   }

   [Test]
   public void LevenshteinDistance_IsCorrect()
   {
      Assert.That(Queastor.LevinsteinDistance("test", "tast"), Is.EqualTo(1));
      Assert.That(Queastor.LevinsteinDistance("taste", "tast"), Is.EqualTo(1));
      Assert.That(Queastor.LevinsteinDistance("toast", "tast"), Is.EqualTo(1));
   }

   [Test]
   public void RemoveFromIndex_RemovesItemCompletely()
   {
      var queastor = new Queastor(new ());
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
      var queastor = new Queastor(new ());
      var item = new MockSearchable(terms:"Alpha");
      queastor.RemoveFromIndex(item); // Should not throw
   }

   [Test]
   public void ModifyInIndex_UpdatesTermsCorrectly_AddsNewTerms()
   {
      var queastor = new Queastor(new ());
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
      var queastor = new Queastor(new ());
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
      var queastor = new Queastor(new ());
      var item = new MockSearchable(1f, "Alpha", "Beta");
      queastor.AddToIndex(item);

      queastor.ModifyInIndex(item, ["Alpha", "Beta"]);

      var results = queastor.Search("Alpha");
      Assert.That(results, Does.Contain(item));

      results = queastor.Search("Beta");
      Assert.That(results, Does.Contain(item));
   }
   

    [Test]
    public void GetClosestMatch_ReturnsClosestTerm()
    {
       var queastor = new Queastor(new ());
        var terms = new List<string> { "apple", "apply", "ape" };
        var closest = queastor.GetClosestMatch("appl", terms);
        Assert.That(closest, Is.EqualTo("apple"));
    }

    [Test]
    public void GetClosestMatch_ReturnsEmptyForNoTerms()
    {
       var queastor = new Queastor(new ());
        var closest = queastor.GetClosestMatch("test", []);
        Assert.That(closest, Is.EqualTo(string.Empty));
    }
}