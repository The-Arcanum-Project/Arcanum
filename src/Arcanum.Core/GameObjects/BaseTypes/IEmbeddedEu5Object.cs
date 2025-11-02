using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.Core.GameObjects.BaseTypes;

/// <summary>
/// Simplified interface for embedded EU5 objects that do not require full search or NUI support.
/// NO Search, NO global items, NO navigation.
/// </summary>
/// <typeparam name="T"></typeparam>
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
   INUINavigation?[] INUI.Navigations => [];
}