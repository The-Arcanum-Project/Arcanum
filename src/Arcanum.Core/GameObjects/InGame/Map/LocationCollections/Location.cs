using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;
using Arcanum.Core.GameObjects.InGame.Pops;
using Nexus.Core;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

[NexusConfig]
[ObjectSaveAs(savingMethod: "LocationSaving")]
public partial class Location
   : IMapInferable, IEu5Object<Location>
{
   #region game/in_game/map_data/named_locations.txt

   [PropertyConfig(isReadonly: true)]
   [SuppressAgs]
   [ToStringArguments("X")]
   [Description("The color of the location in the map data.")]
   [DefaultValue(null)]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   #endregion

   #region main_menu/setup/start

   [SaveAs]
   [ParseAs("define_pop", isShatteredList: true, itemNodeType: AstNodeType.BlockNode, isEmbedded: true)]
   [Description("The pops residing in this location.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<PopDefinition> Pops { get; set; } = [];

   [SaveAs]
   [ParseAs(Globals.DO_NOT_PARSE_ME,
            isShatteredList: true,
            itemNodeType: AstNodeType.ContentNode,
            iEu5KeyType: typeof(Institution))]
   [Description("The institution presences in this location.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<InstitutionPresence> InstitutionPresences { get; set; } = [];

   [SaveAs]
   [ParseAs("rank")]
   [Description("The rank of this location.")]
   [DefaultValue(null)]
   public LocationRank Rank { get; set; } = LocationRank.Empty;

   [SaveAs]
   [ParseAs("town_setup")]
   [Description("The town setup associated with this location.")]
   [DefaultValue(null)]
   public TownSetup TownSetup { get; set; } = TownSetup.Empty;

   [SaveAs]
   [ParseAs("prosperity")]
   [Description("The prosperity of this location.")]
   [DefaultValue(0f)]
   public float Prosperity { get; set; }

   #endregion

   #region game/map_data/location_templates.txt

   [PropertyConfig(isInlined: true)]
   [SuppressAgs]
   [Description("The template data associated with this location.")]
   [DefaultValue(null)]
   public LocationTemplateData TemplateData { get; set; } = LocationTemplateData.Empty;

   #endregion

   public List<Location> GetLocations() => throw new NotImplementedException();

   public LocationCollectionType LcType => LocationCollectionType.Location;

   public static Dictionary<string, Location> GetGlobalItems() => Globals.Locations;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs.Cast<IEu5Object>().ToList();
   public List<Location> GetRelevantLocations(IEu5Object[] items) => items.Cast<Location>().ToList();
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Locations;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.LocationSettings;
   public INUINavigation[] Navigations { get; } = [];
   public string GetNamespace => "Map.Location";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects |
                                 IQueastorSearchSettings.DefaultCategories.MapObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.Location;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Location Empty { get; } = new() { UniqueId = "Empty_Arcanum_Location" };

   [SaveAs(isEmbeddedObject: true)]
   [ParseAs("null", ignore: true)]
   [Description("The Province this Location belongs to.")]
   [DefaultValue(null)]
   [SuppressAgs]
   [PropertyConfig(aggregateLinktParent: "Locations", aggreateLinkType: AggregateLinkType.Child)]
   public Province Province { get; set; } = Province.Empty;

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