using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Arcanum.API.Attributes;

namespace Arcanum.API.UtilServices.Search.SearchableSetting;

public abstract partial class SearchableSettings : ISearchable
{
   private readonly Type _topLevelType;

   protected SearchableSettings(Type topLevelType, List<string>? searchTerms = null)
   {
      _topLevelType = topLevelType ?? throw new ArgumentNullException(nameof(topLevelType));
      GetNamespace = BuildNameSpace(topLevelType, GetType().FullName!, ((ISearchable)this).NamespaceSeparator);
      SearchTerms = searchTerms ?? GenerateSearchTerms(GetType().Name);
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

         var searchTerms = GenerateSearchTerms(pd.Name);
         var nameSpace = BuildNameSpace(topLevelType,
                                        $"{pd.DeclaringType?.FullName}.{pd.Name}",
                                        ((ISearchable)this).NamespaceSeparator);

         var searchable = new SearchableSettingsProperty(pd.GetCustomAttribute<
                                                               IconPathSettingsAttribute>()
                                                          ?.IconPath,
                                                         nameSpace,
                                                         pd.Name,
                                                         searchTerms);

         searchables.Add(searchable);
      }

      return searchables;
   }

   /// <summary>
   /// We also want to add each part of the camelCase name as a search term and
   /// a substring of the full name getting longer with each camelCase part <br/>
   /// example: ThisIsAnExample -> this, is, an, example, thisIs, thisIsAn, thisIsAnExample
   /// </summary>
   /// <returns></returns>
   private static List<string> GenerateSearchTerms(string desc)
   {
      var parts = CamelCaseSplitter().Split(desc);

      List<string> terms = [..parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.ToLower()),];

      // Build progressive substrings
      for (var i = 2; i <= parts.Length; i++) // start at 2 to avoid first match being empty
         terms.Add(string.Join("", parts[..i]));

      return terms.ToList();
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

   public void OnSearchSelected()
   {
      throw new NotImplementedException();
   }

   [JsonIgnore]
   [IgnoreSettingProperty]
   [IgnoreInPropertyGrid]
   public ISearchResult VisualRepresentation => new SearchResultItem(null, ResultName, GetNamespace);

   [JsonIgnore]
   [IgnoreSettingProperty]
   [IgnoreInPropertyGrid]
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.Settings;

   [GeneratedRegex("(?=[A-Z])")]
   private static partial Regex CamelCaseSplitter();

   public abstract float GetRelevanceScore(string query);
}