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
public partial class Continent
   : IMapInferable, IEu5Object<Continent>, IIndexRandomColor
{
   public Continent()
   {
      SuperRegions = GetEmptyAggregateLink_Continent_SuperRegion();
   }

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                                               .Continent)!)
                                                                          .Distinct()
                                                                          .ToList();

   [DefaultValue(null)]
   [SuppressAgs]
   [SaveAs(isEmbeddedObject: true)]
   [ParseAs("null", ignore: true)]
   [PropertyConfig(defaultValueMethod: "GetEmptyAggregateLink_Continent_SuperRegion")]
   public AggregateLink<SuperRegion> SuperRegions { get; set; }

   public AggregateLink<SuperRegion> GetEmptyAggregateLink_Continent_SuperRegion()
   {
      return new(SuperRegion.Field.Continent, this);
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      List<Location> locations = [];

      foreach (var item in items)
         if (item is Continent cn && cn.SuperRegions.Count > 0)
            locations.AddRange(cn.SuperRegions[0].GetRelevantLocations(cn.SuperRegions.Cast<IEu5Object>().ToArray()));
      return locations;
   }

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ContinentSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Continent> GetGlobalItems() => Globals.Continents;
   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Continents;
   public string GetNamespace => $"Map.{nameof(Continent)}";
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ContinentAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Continent Empty { get; } = new() { UniqueId = "Arcanum_Empty_Continent" };

   // IIndexRandomColor Implementation
   public int Index { get; set; }
}