using System.Reflection;
using System.Text.Json.Serialization;
using Arcanum.API.Attributes;

namespace Arcanum.API.UtilServices.Search.SearchableSetting;

public abstract class SearchableSettings : ISearchable
{
   private readonly Type _topLevelType;
   private readonly object _root;

   protected SearchableSettings(object root, Type topLevelType, List<string>? searchTerms = null)
   {
      _topLevelType = topLevelType ?? throw new ArgumentNullException(nameof(topLevelType));
      GetNamespace = BuildNameSpace(topLevelType, GetType().FullName!, ((ISearchable)this).NamespaceSeparator);
      SearchTerms = searchTerms ?? SearchHelper.GenerateSearchTerms(GetType().Name);
      _root = root;
   }

   public List<ISearchable> GetAllSearchableObjects()
   {
      return [..GenerateSearchableProperties(this, _topLevelType), this];
   }

   private List<ISearchable> GenerateSearchableProperties(object settingsObj, Type topLevelType)
   {
      List<ISearchable> searchables = [];
      foreach (var pd in settingsObj.GetType().GetProperties())
      {
         if (pd.GetCustomAttribute<IgnoreSettingPropertyAttribute>() != null)
            continue;

         var searchTerms = SearchHelper.GenerateSearchTerms(pd.Name);
         var nameSpace = BuildNameSpace(topLevelType,
                                        $"{pd.DeclaringType?.FullName}.{pd.Name}",
                                        ((ISearchable)this).NamespaceSeparator);

         var searchable = new SearchableSettingsProperty(pd.GetCustomAttribute<
                                                               IconPathSettingsAttribute>()
                                                          ?.IconPath,
                                                         nameSpace,
                                                         pd.Name,
                                                         searchTerms,
                                                         _root,
                                                         settingsObj,
                                                         pd.Name);

         searchables.Add(searchable);
      }

      return searchables;
   }

   private static string BuildNameSpace(Type topLevelType, string targetNameSpace, char separator)
   {
      var namespaceName = topLevelType.Namespace;
      if (namespaceName == null)
         return nameof(SearchableSettings);

      if (targetNameSpace.StartsWith(namespaceName))
         return targetNameSpace[(namespaceName.Length + 1)..]
           .Replace('.', separator); // +1 for the dot

      return targetNameSpace.Replace('.', separator);
   }

   [IgnoreInPropertyGrid]
   [JsonIgnore]
   [IgnoreSettingProperty]
   public string GetNamespace { get; }
   [IgnoreInPropertyGrid]
   [JsonIgnore]
   [IgnoreSettingProperty]
   public abstract string ResultName { get; }
   [IgnoreInPropertyGrid]
   [JsonIgnore]
   [IgnoreSettingProperty]
   public List<string> SearchTerms { get; set; }

   public abstract void OnSearchSelected();

   [JsonIgnore]
   [IgnoreSettingProperty]
   [IgnoreInPropertyGrid]
   public ISearchResult VisualRepresentation => new SearchResultItem(null, ResultName, GetNamespace);

   [JsonIgnore]
   [IgnoreSettingProperty]
   [IgnoreInPropertyGrid]
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.Settings;

   public abstract float GetRelevanceScore(string query);
}