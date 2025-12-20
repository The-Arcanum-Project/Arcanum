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
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class Area : IMapInferable, IEu5Object<Area>, IIndexRandomColor
{
   public Area()
   {
      Provinces = GetEmptyAggregateLink_Area_Province();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.AreaSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Area> GetGlobalItems() => Globals.Areas;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                                               .Area)!)
                                                                          .Distinct()
                                                                          .ToList();

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Areas;
   public string GetNamespace => $"Map.{nameof(Area)}";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));

   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;

   public AgsSettings AgsSettings => Config.Settings.AgsSettings.AreaAgsSettings;

   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId { get; set; } = string.Empty;

   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Area Empty { get; } = new() { UniqueId = "Arcanum_Empty_Area" };

   [SaveAs(isEmbeddedObject: true)]
   [ParseAs("null", ignore: true)]
   [Description("The Region this Area belongs to.")]
   [DefaultValue(null)]
   [SuppressAgs]
   public Region Region
   {
      get;
      set
      {
         if (field != Region.Empty)
            field.Areas._removeFromChild(this);
         if (value != Region.Empty)
            value.Areas._addFromChild(this);

         field = value;
      }
   } = Region.Empty;

   [DefaultValue(null)]
   [SuppressAgs]
   [SaveAs(isEmbeddedObject: true)]
   [Description("The Provinces that are part of this Area.")]
   [ParseAs("-", ignore: true)]
   [PropertyConfig(defaultValueMethod: "GetEmptyAggregateLink_Area_Province")]
   public AggregateLink<Province> Provinces { get; set; }

   protected AggregateLink<Province> GetEmptyAggregateLink_Area_Province()
   {
      return new(Province.Field.Area, Field.Provinces, this);
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      List<Location> locations = [];

      foreach (var item in items)
         if (item is Area { Provinces.Count: > 0 } cn)
            locations.AddRange(cn.Provinces[0].GetRelevantLocations(cn.Provinces.Cast<IEu5Object>().ToArray()));
      return locations;
   }

   public LocationCollectionType LcType => LocationCollectionType.Area;

   // IIndexRandomColor Implementation
   public int Index { get; set; }
}