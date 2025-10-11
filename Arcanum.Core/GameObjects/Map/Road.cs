using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs(savingMethod: "RoadSavingMethod")]
public partial class Road : IEu5Object<Road>
{
   [SuppressAgs]
   [Description("The starting location of the road.")]
   [DefaultValue(null)]
   public Location StartLocation { get; set; } = Location.Empty;
   [SuppressAgs]
   [Description("The ending location of the road.")]
   [DefaultValue(null)]
   public Location EndLocation { get; set; } = Location.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.RoadSettings;
   public INUINavigation[] Navigations =>
   [
      new NUINavigation(StartLocation, $"Start: {StartLocation.UniqueId}"),
      new NUINavigation(EndLocation, $"End: {EndLocation.UniqueId}"),
   ];

   public static Dictionary<string, Road> GetGlobalItems() => Globals.Roads;
   public static Road Empty { get; } = new() { StartLocation = Location.Empty, EndLocation = Location.Empty };
   public string GetNamespace => "Map.Roads";
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [StartLocation.UniqueId, EndLocation.UniqueId];

   public void OnSearchSelected()
   {
      UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.RoadAgsSettings;
   public string SavingKey => string.Empty;
   public string UniqueId
   {
      get => $"{StartLocation.UniqueId}_{EndLocation.UniqueId}";
      set => throw new NotSupportedException();
   }
   public Eu5FileObj Source { get; set; } = null!;
}