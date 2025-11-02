using Arcanum.API.UtilServices.Search;
using Arcanum.Core.Registry;

namespace Arcanum.UI.Saving.Backend;

public class FileSearchSettings
{
   public IQueastorSearchSettings.SearchModes SearchMode { get; set; } = IQueastorSearchSettings.SearchModes.Default;
   public IQueastorSearchSettings.SortingOptions SortingOption { get; set; } =
      IQueastorSearchSettings.SortingOptions.Relevance;
   public Enum[] AvailableCategories { get; set; } =
      Enum.GetValues<Eu5ObjectsRegistry.Eu5ObjectsEnum>().Cast<Enum>().ToArray();
   public bool WholeWord { get; set; }
   public int MaxLevinsteinDistance { get; set; } = 2;

   public void ApplySettings(IQueastorSearchSettings settings)
   {
      settings.SearchMode = SearchMode;
      settings.SortingOption = SortingOption;
      settings.WholeWord = WholeWord;
      settings.MaxLevinsteinDistance = MaxLevinsteinDistance;
   }
}