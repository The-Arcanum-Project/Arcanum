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
public partial class Province
   : IMapInferable, IEu5Object<Province>, IIndexRandomColor
{
   public Province()
   {
      Locations = GetEmptyAggregateLink_Province_Location();
   }

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ProvinceSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Province> GetGlobalItems() => Globals.Provinces;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                                               .Province)!)
                                                                          .Distinct()
                                                                          .ToList();

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Provinces;
   public static Province Empty { get; } = new() { UniqueId = "Empty Province" };
   public string GetNamespace => "Map.Province";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.Province;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public LocationCollectionType LcType => LocationCollectionType.Province;

   [SaveAs(isEmbeddedObject: true)]
   [ParseAs("null", ignore: true)]
   [Description("The Area this Province belongs to.")]
   [DefaultValue(null)]
   [SuppressAgs]
   public Area Area
   {
      get;
      set
      {
         if (field != Area.Empty)
            field.Provinces._removeFromChild(this);
         if (value != Area.Empty)
            value.Provinces._addFromChild(this);

         field = value;
      }
   } = Area.Empty;

   [DefaultValue(null)]
   [SuppressAgs]
   [SaveAs(isEmbeddedObject: true)]
   [Description("The Locations that are part of this Province.")]
   [ParseAs("-", ignore: true)]
   [PropertyConfig(defaultValueMethod: "GetEmptyAggregateLink_Province_Location")]
   public AggregateLink<Location> Locations { get; set; }

   protected AggregateLink<Location> GetEmptyAggregateLink_Province_Location()
   {
      return new(Location.Field.Province, Field.Locations, this);
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      List<Location> locations = [];

      foreach (var item in items)
         if (item is Province { Locations.Count: > 0 } cn)
            locations.AddRange(cn.Locations[0].GetRelevantLocations(cn.Locations.Cast<IEu5Object>().ToArray()));
      return locations;
   }

   // IIndexRandomColor Implementation
   public int Index { get; set; }
}