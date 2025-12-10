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
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.Utils.DataStructures;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class SuperRegion
   : IMapInferable, IEu5Object<SuperRegion>, IIndexRandomColor
{
   public SuperRegion()
   {
      Regions = GetEmptyAggregateLink_SuperRegion_Region();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.SuperRegionSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, SuperRegion> GetGlobalItems() => Globals.SuperRegions;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                                               .SuperRegion)!)
                                                                          .Distinct()
                                                                          .ToList();

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.SuperRegions;
   public string GetNamespace => "Map.Superregion";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SuperRegionAgsSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static SuperRegion Empty { get; } = new () { UniqueId = "Arcanum_Empty_SuperRegion" };
   public LocationCollectionType LcType => LocationCollectionType.SuperRegion;

   [ParseAs("null", ignore: true)]
   [Description("The Continent this SuperRegion belongs to.")]
   [DefaultValue(null)]
   [SuppressAgs]
   public Continent Continent
   {
      get => field;

      set
      {
         if (field != Continent.Empty)
            field.SuperRegions._removeFromChild(this);
         if (value != Continent.Empty)
            value.SuperRegions._addFromChild(this);

         field = value;
      }
   } = Continent.Empty;

   [DefaultValue(null)]
   [SuppressAgs]
   [SaveAs(isEmbeddedObject: true)]
   [Description("The Regions that are part of this SuperRegion.")]
   [ParseAs("-", ignore: true)]
   [PropertyConfig(defaultValueMethod: "GetEmptyAggregateLink_SuperRegion_Region")]
   public AggregateLink<Region> Regions { get; set; }

   protected AggregateLink<Region> GetEmptyAggregateLink_SuperRegion_Region()
   {
      return new(Region.Field.SuperRegion, Field.Regions, this);
   }
   
   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      List<Location> locations = [];

      foreach (var item in items)
         if (item is SuperRegion { Regions.Count: > 0 } cn)
            locations.AddRange(cn.Regions[0].GetRelevantLocations(cn.Regions.Cast<IEu5Object>().ToArray()));
      return locations;
   }
   
   // IIndexRandomColor Implementation
   public int Index { get; set; }
}