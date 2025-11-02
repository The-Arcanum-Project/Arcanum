using Common.UI;
using Common.Utils.PropertyUtils;

namespace Arcanum.API.UtilServices.Search.SearchableSetting;

public class SearchableSettingsProperty(string? iconPath,
                                        string nSpace,
                                        string resultName,
                                        List<string> searchTerms,
                                        object root,
                                        object parent,
                                        string propName)
   : ISearchable
{
   [IgnoreSettingProperty]
   public string GetNamespace { get; } = nSpace;
   [IgnoreSettingProperty]
   public string ResultName { get; } = resultName;

   [IgnoreSettingProperty]
   public List<string> SearchTerms { get; set; } = searchTerms;

   public void OnSearchSelected()
   {
      var info = parent.GetType().GetProperty(propName);
      if (info == null)
         throw new InvalidOperationException($"Property '{propName}' not found in type '{parent.GetType().Name}'.");

      var path = PropertyPathBuilder.GetPathToProperty(root, info);
      UIHandle.Instance.PopUpHandle.NavigateToSetting(path);
   }

   [IgnoreSettingProperty]
   public ISearchResult VisualRepresentation => new SearchResultItem(iconPath, ResultName, GetNamespace);

   [IgnoreSettingProperty]
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.Settings;
}