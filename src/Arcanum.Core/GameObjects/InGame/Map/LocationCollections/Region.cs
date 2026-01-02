using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections.BaseClasses;
using Arcanum.Core.Utils.DataStructures;
using Nexus.Core;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class Region : IMapInferable, IEu5Object<Region>, IIndexRandomColor
{
   public Region()
   {
      Areas = GetEmptyAggregateLink_Region_Area();
   }

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.RegionSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Region> GetGlobalItems() => Globals.Regions;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                                               .Region))
                                                                          .Distinct()
                                                                          .ToList();

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Regions;
   public string GetNamespace => "Map.Region";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));

   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;

   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.Region;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Region Empty { get; } = new() { UniqueId = "Arcanum_Empty_Region" };

   [SaveAs(isEmbeddedObject: true)]
   [ParseAs("null", ignore: true)]
   [Description("The SuperRegion this Region belongs to.")]
   [DefaultValue(null)]
   [SuppressAgs]
   [PropertyConfig(aggregateLinktParent: "Regions", aggreateLinkType: AggregateLinkType.Child, isRequired: true)]
   public SuperRegion SuperRegion { get; set; } = SuperRegion.Empty;

   [DefaultValue(null)]
   [SuppressAgs]
   [SaveAs(isEmbeddedObject: true)]
   [Description("The Areas that are part of this Region.")]
   [ParseAs("-", ignore: true)]
   [PropertyConfig(defaultValueMethod: "GetEmptyAggregateLink_Region_Area")]
   public AggregateLink<Area> Areas { get; set; }

   protected AggregateLink<Area> GetEmptyAggregateLink_Region_Area()
   {
      return new(Area.Field.Region, Field.Areas, this);
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      List<Location> locations = [];

      foreach (var item in items)
         if (item is Region { Areas.Count: > 0 } cn)
            locations.AddRange(cn.Areas[0].GetRelevantLocations(cn.Areas.Cast<IEu5Object>().ToArray()));
      return locations;
   }

   public LocationCollectionType LcType => LocationCollectionType.Region;

   // IIndexRandomColor Implementation
   public int Index { get; set; }
}