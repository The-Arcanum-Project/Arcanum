using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
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
using Arcanum.Core.GameObjects.InGame.Map;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Religious;

[NexusConfig]
[ObjectSaveAs]
public partial class ReligionGroup : IEu5Object<ReligionGroup>, IMapInferable
{
   #region Nexus Properties

   [ParseAs("color")]
   [DefaultValue(null)]
   [SaveAs]
   [Description("Color associated with this ReligionGroup.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [ParseAs("convert_slaves_at_start")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether slaves should be converted at the start for this ReligionGroup.")]
   public bool ConvertSlavesAtStart { get; set; }

   [ParseAs("allow_slaves_of_same_group")]
   [DefaultValue(false)]
   [SaveAs]
   [Description("Indicates whether slaves of the same ReligionGroup are allowed.")]
   public bool AllowSlavesOfSameGroup { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to members of this ReligionGroup.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this ReligionGroup. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligionGroup)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligionGroupSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligionGroupAgsSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static Dictionary<string, ReligionGroup> GetGlobalItems() => Globals.ReligionGroups;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static ReligionGroup Empty { get; } = new() { UniqueId = "Arcanum_Empty_ReligionGroup" };

   #endregion

   #region IMapInferable

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.ReligionGroup;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
   {
      HashSet<IEu5Object> items = [];
      foreach (var loc in sLocs)
      {
         if (loc.TemplateData == LocationTemplateData.Empty && loc.TemplateData.Religion.Group != Empty)
            continue;

         items.Add(loc.TemplateData.Religion.Group);
      }

      return items.ToList();
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is ReligionGroup));
      var objs = items.Cast<ReligionGroup>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (objs.Contains(loc.TemplateData.Religion.Group) &&
             loc.TemplateData != LocationTemplateData.Empty &&
             loc.TemplateData.Religion.Group != Empty)
            locations.Add(loc);

      return locations;
   }

   #endregion
}