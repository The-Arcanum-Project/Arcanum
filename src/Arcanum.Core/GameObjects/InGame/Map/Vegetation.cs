using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.Core.GameObjects.InGame.Map;

[NexusConfig]
[ObjectSaveAs]
public partial class Vegetation : IEu5Object<Vegetation>, IMapInferable
{
#pragma warning disable AGS004
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("color")]
   [Description("The color associated with this vegetation type used on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("debug_color")]
   [Description("The debug color associated with this vegetation type used in the vegetation_screenshot mapmode.")]
   public JominiColor DebugColor { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("movement_cost")]
   [Description("The movement cost modifier for units moving through this vegetation type.")]
   public float MovementCost { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("has_sand")]
   [Description("Whether this vegetation type includes sandy terrain, affecting certain gameplay mechanics.")]
   public bool HasSand { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("defender")]
   [Description("The defender bonus provided by this vegetation type in combat scenarios.")]
   public int DefenderDice { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("location_modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The location modifier applied to provinces with this climate.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("audio_tags", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The audio tags associated with this climate.")]
   public ObservableRangeCollection<AudioTag> AudioTags { get; set; } = [];

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.VegetationSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Vegetation Empty { get; } = new() { UniqueId = "Arcanum_Empty_Vegetation" };
   public static Dictionary<string, Vegetation> GetGlobalItems() => Globals.Vegetation;

   #endregion

   #region ISearchable

   public string GetNamespace => $"Map.{nameof(Vegetation)}";
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected()
   {
      SelectionManager.Eu5ObjectSelectedInSearch(this);
   }

   public ISearchResult VisualRepresentation { get; } = new SearchResultItem(null, "Vegetation", string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;

   #endregion

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.VegetationAgsSettings;
   public string SavingKey => UniqueId;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;

   #region IMapInferable

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Vegetation;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
   {
      HashSet<IEu5Object> items = [];
      foreach (var loc in sLocs)
      {
         if (loc.TemplateData == LocationTemplateData.Empty && loc.TemplateData.Vegetation != Empty)
            continue;

         items.Add(loc.TemplateData.Vegetation);
      }

      return items.ToList();
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is Vegetation));
      var objs = items.Cast<Vegetation>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (objs.Contains(loc.TemplateData.Vegetation) &&
             loc.TemplateData != LocationTemplateData.Empty &&
             loc.TemplateData.Vegetation != Empty)
            locations.Add(loc);

      return locations;
   }

   #endregion
}