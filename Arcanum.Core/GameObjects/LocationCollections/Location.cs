using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GameObjects.Pops;
using Common.UI;

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class Location
   : IMapInferable<Location>, IEu5Object<Location>, ILocation
{
   #region game/in_game/map_data/named_locations.txt

   [SuppressAgs]
   [ToStringArguments("X")]
   [Description("The color of the location in the map data.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   #endregion

   #region Market: game/main_menu/setup/start

   public bool HasMarket => Globals.Markets.ContainsKey($"{UniqueId}_market");

   #endregion

   #region Pops: game/main_menu/setup/start/06_pops.txt

   [SuppressAgs]
   [Description("The pops residing in this location.")]
   public ObservableRangeCollection<PopDefinition> Pops { get; set; } = [];

   #endregion

   public override string ToString() => UniqueId;
   public ICollection<Location> GetLocations() => throw new NotImplementedException();

   public LocationCollectionType LcType => LocationCollectionType.Location;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];

   public static Dictionary<string, Location> GetGlobalItems() => Globals.Locations;

   public static List<Location> GetInferredList(IEnumerable<Location> sLocs) => sLocs.ToList();
   public static IMapMode GetMapMode { get; } = new BaseMapMode();

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.LocationSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = this.GetFirstParentOfType(LocationCollectionType.Province);
         if (parent != null)
            navigations.Add(new NUINavigation(parent, $"Province: {parent.UniqueId}"));

         navigations.Add(null);
         navigations.AddRange(Pops.Select(pop => new NUINavigation(pop,
                                                                   $"Pop: {pop.PopType} ({pop.Culture}, {pop.Religion})")));

         return navigations.ToArray()!;
      }
   }
   public string GetNamespace => throw new NotImplementedException();

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.GameObjects | IQueastorSearchSettings.Category.MapObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.LocationAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public static Location Empty => new() { UniqueId = "Empty_Arcanum_Location" };

   #region Map Management

   [SuppressAgs]
   public int ColorIndex { get; set; } = -1;

   #endregion
}