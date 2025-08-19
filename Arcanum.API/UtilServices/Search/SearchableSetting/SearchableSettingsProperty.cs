namespace Arcanum.API.UtilServices.Search.SearchableSetting;

public class SearchableSettingsProperty(string? iconPath,
                                        string nSpace,
                                        string resultName,
                                        List<string> searchTerms)
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
      //TODO: @Minnator Implement navigation in Settings UI and PropertyGrids
   }

   [IgnoreSettingProperty]
   public ISearchResult VisualRepresentation => new SearchResultItem(iconPath, ResultName, GetNamespace);

   [IgnoreSettingProperty]
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.Settings;
}