using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class Province
   : IMapInferable, IEu5Object<Province>, ILocation, ILocationCollection<Location>, IIndexRandomColor
{
   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ProvinceSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = this.GetFirstParentOfType(LocationCollectionType.Area);
         if (parent != null)
            navigations.Add(new NUINavigation(parent, $"Area: {parent.UniqueId}"));

         if (LocationChildren.Count > 0)
            navigations.Add(null);

         foreach (var location in LocationChildren)
            navigations.Add(new NUINavigation(location, $"Location: {location.UniqueId}"));

         return navigations.ToArray()!;
      }
   }
   public static Dictionary<string, Province> GetGlobalItems() => Globals.Provinces;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                 .GetFirstParentOfType(LocationCollectionType
                                                                                        .Province)!)
                                                                          .Distinct()
                                                                          .ToList();

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      var typedItems = items.Cast<Province>();
      List<Location> locations = [];
      foreach (var item in typedItems)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Locations;
   public static Province Empty { get; } = new() { UniqueId = "Empty Province" };
   public string GetNamespace => "Map.Province";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ProvinceAgsSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public List<Location> GetLocations() => LocationChildren.ToList();

   public LocationCollectionType LcType => LocationCollectionType.Province;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(collectionAsPureIdentifierList: true)]
   public ObservableRangeCollection<Location> LocationChildren { get; set; } = [];

   // IIndexRandomColor Implementation
   public int Index { get; set; }
}