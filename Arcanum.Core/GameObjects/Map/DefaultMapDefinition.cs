using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.PropertyHelpers;
using Common.UI;

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs]
public partial class DefaultMapDefinition : IEu5Object<DefaultMapDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("provinces")]
   [DefaultValue("locations.png")]
   [Description("The filename of the location map image.")]
   public string ProvinceFileName { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("rivers")]
   [DefaultValue("rivers.png")]
   [Description("The filename of the rivers map image.")]
   public string Rivers { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("topology")]
   [Description("The filename of the heightmap image.")]
   [DefaultValue("heightmap.heightmap")]
   public string HeightMap { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("adjacencies")]
   [Description("The filename of the adjacencies CSV file.")]
   [DefaultValue("adjacencies.csv")]
   public string Adjacencies { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("setup")]
   [Description("The filename of the setup definitions file.")]
   [DefaultValue("definitions.txt")]
   public string Setup { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("ports")]
   [Description("The filename of the ports CSV file.")]
   [DefaultValue("ports.csv")]
   public string Ports { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("location_templates")]
   [Description("The filename of the location templates CSV file.")]
   [DefaultValue("locations_templates.csv")]
   public string LocationsTemplates { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("wrap_x")]
   [DefaultValue(true)]
   [Description("Whether the map wraps around the X axis (horizontally).")]
   public bool WrapX { get; set; } = true;

   [SaveAs]
   [ParseAs("equator_y")]
   [DefaultValue(-1)]
   [Description("At what Y coordinate the equator is located on the map image.")]
   public int EquatorY { get; set; } = -1;

   [SaveAs]
   [ParseAs("sound_toll", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("List of pairs of locations that are connected by sound tolls.")]
   public ObservableRangeCollection<SoundToll> SoundTolls { get; set; } = [];

   [SaveAs]
   [ParseAs("volcanoes", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that contain volcanoes.")]
   public ObservableHashSet<Location> Volcanoes { get; set; } = [];

   [SaveAs]
   [ParseAs("earthquakes", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are prone to earthquakes.")]
   public ObservableHashSet<Location> Earthquakes { get; set; } = [];

   [SaveAs]
   [ParseAs("sea_zones", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are sea zones.")]
   public ObservableHashSet<Location> SeaZones { get; set; } = [];

   [SaveAs]
   [ParseAs("lakes", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are lakes.")]
   public ObservableHashSet<Location> Lakes { get; set; } = [];

   [SaveAs]
   [ParseAs("non_ownable", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that cannot be owned by any country.")]
   public ObservableHashSet<Location> NotOwnable { get; set; } = [];

   [SaveAs]
   [ParseAs("impassable_mountains", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are impassable due to mountains.")]
   public ObservableHashSet<Location> ImpassableMountains { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this DefaultMapDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.{nameof(DefaultMapDefinition)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.DefaultMapDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.DefaultMapDefinitionAgsSettings;

   public static Dictionary<string, DefaultMapDefinition> GetGlobalItems() => new()
   {
      { "Default Map", Globals.DefaultMapDefinition },
   };

   public static DefaultMapDefinition Empty { get; } = new() { UniqueId = "Arcanum_Empty_DefaultMapDefinition" };

   public override string ToString() => UniqueId;

   #endregion
}