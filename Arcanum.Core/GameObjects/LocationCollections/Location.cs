using System.Collections;
using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Pops;
using Common.UI;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class Location
   : IMapInferable<Location>, IEu5Object<Location>, ILocation
{
   #region game/in_game/map_data/named_locations.txt

   [SuppressAgs]
   [ToStringArguments("X")]
   [Description("The color of the location in the map data.")]
   [DefaultValue(null)]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   #endregion

   #region Market: game/main_menu/setup/start

   public bool HasMarket => Globals.Markets.ContainsKey($"{UniqueId}_market");

   #endregion

   #region Pops: game/main_menu/setup/start/06_pops.txt

   [SuppressAgs]
   [Description("The pops residing in this location.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<PopDefinition> Pops { get; set; } = [];

   #endregion

   #region game/map_data/location_templates.txt

   [SuppressAgs]
   [Description("The template data associated with this location.")]
   [DefaultValue(null)]
   public LocationTemplateData TemplateData { get; set; } = LocationTemplateData.Empty;

   #endregion

   public override string ToString() => UniqueId;
   public List<Location> GetLocations() => throw new NotImplementedException();

   public LocationCollectionType LcType => LocationCollectionType.Location;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];

   public static Dictionary<string, Location> GetGlobalItems() => Globals.Locations;

   public static List<Location> GetInferredList(IEnumerable<Location> sLocs) => sLocs.ToList();
   public static List<Location> GetRelevantLocations(IEnumerable items) => items.Cast<Location>().ToList();

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
   public string GetNamespace => "Map.Location";

   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects |
                                 IQueastorSearchSettings.DefaultCategories.MapObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.LocationAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Location Empty => new() { UniqueId = "Empty_Arcanum_Location" };

   #region Map Management

   [SuppressAgs]
   [IgnoreModifiable]
   public int ColorIndex { get; set; } = -1;

   [IgnoreModifiable]
   public Polygon[] Polygons { get; set; } = [];

   [IgnoreModifiable]
   public RectangleF Bounds { get; set; } = RectangleF.Empty;

   #endregion
}