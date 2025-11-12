using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs(savingMethod: "DefaultMapDefinitionSaving")]
public partial class DefaultMapDefinition : IEu5Object<DefaultMapDefinition>
{
   #region Nexus Properties

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("provinces")]
   [DefaultValue("locations.png")]
   [Description("The filename of the location map image.")]
   public string ProvinceFileName { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("rivers")]
   [DefaultValue("rivers.png")]
   [Description("The filename of the rivers map image.")]
   public string Rivers { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("topology")]
   [Description("The filename of the heightmap image.")]
   [DefaultValue("heightmap.heightmap")]
   public string HeightMap { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("adjacencies")]
   [Description("The filename of the adjacencies CSV file.")]
   [DefaultValue("adjacencies.csv")]
   public string Adjacencies { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("setup")]
   [Description("The filename of the setup definitions file.")]
   [DefaultValue("definitions.txt")]
   public string Setup { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("ports")]
   [Description("The filename of the ports CSV file.")]
   [DefaultValue("ports.csv")]
   public string Ports { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("location_templates")]
   [Description("The filename of the location templates CSV file.")]
   [DefaultValue("locations_templates.csv")]
   public string LocationsTemplates { get; set; } = string.Empty;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("wrap_x")]
   [DefaultValue(true)]
   [Description("Whether the map wraps around the X axis (horizontally).")]
   public bool WrapX { get; set; } = true;

   [SaveAs, PropertyConfig(isRequired: true)]
   [ParseAs("equator_y")]
   [DefaultValue(-1)]
   [Description("At what Y coordinate the equator is located on the map image.")]
   public int EquatorY { get; set; } = -1;

   [SaveAs]
   [ParseAs("sound_toll", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("List of pairs of locations that are connected by sound tolls.")]
   public ObservableRangeCollection<SoundToll> SoundTolls { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("volcanoes", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that contain volcanoes.")]
   public ObservableHashSet<Location> Volcanoes { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("earthquakes", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are prone to earthquakes.")]
   public ObservableHashSet<Location> Earthquakes { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("sea_zones", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are sea zones.")]
   public ObservableHashSet<Location> SeaZones { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("lakes", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are lakes.")]
   public ObservableHashSet<Location> Lakes { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("non_ownable", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that cannot be owned by any country.")]
   public ObservableHashSet<Location> NotOwnable { get; set; } = [];

   [SaveAs(SavingValueType.Identifier, saveEmbeddedAsIdentifier: false)]
   [ParseAs("impassable_mountains", itemNodeType: AstNodeType.KeyOnlyNode)]
   [DefaultValue(null)]
   [Description("List of locations that are impassable due to mountains.")]
   public ObservableHashSet<Location> ImpassableMountains { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this DefaultMapDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.{nameof(DefaultMapDefinition)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
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