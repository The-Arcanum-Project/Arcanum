using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.Core.GameObjects.BaseTypes;

public interface IEmbeddedEu5Object<T> : IEu5Object<T> where T : IEu5Object<T>, new()
{
   string ISearchable.GetNamespace => $"Embedded.{GetType().Name}";
   string ISearchable.ResultName => string.Empty;
   List<string> ISearchable.SearchTerms => [];

   void ISearchable.OnSearchSelected() => throw new NotSupportedException();
   float ISearchable.GetRelevanceScore(string query, string key) => throw new NotSupportedException();
   ISearchResult ISearchable.VisualRepresentation => throw new NotSupportedException();
   Enum ISearchable.SearchCategory => throw new NotSupportedException();
   bool INUI.IsReadonly => false;
   static Dictionary<string, T> IEu5ObjectProvider<T>.GetGlobalItems() => [];
   static T IEmpty<T>.Empty => new() { UniqueId = $"Arcanum_Empty_{typeof(T).Name}" };
   INUINavigation?[] INUI.Navigations => [];
}