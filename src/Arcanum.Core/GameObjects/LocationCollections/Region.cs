using System.Collections;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.LocationCollections;

[NexusConfig]
[ObjectSaveAs]
public partial class Region : IMapInferable, IEu5Object<Region>, ILocation, ILocationCollection<Area>, IIndexRandomColor
{
   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.RegionSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = this.GetFirstParentOfType(LocationCollectionType.SuperRegion);
         if (parent != null)
            navigations.Add(new NUINavigation(parent, $"SuperRegion: {parent.UniqueId}"));

         if (LocationChildren.Count > 0)
            navigations.Add(null);

         foreach (var location in LocationChildren)
            navigations.Add(new NUINavigation(location, $"Region: {location.UniqueId}"));

         return navigations.ToArray()!;
      }
   }
   public static Dictionary<string, Region> GetGlobalItems() => Globals.Regions;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                 .GetFirstParentOfType(LocationCollectionType
                                                                                        .Area)!)
                                                                          .Distinct()
                                                                          .ToList();

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      var typedItems = items.Cast<Region>();
      List<Location> locations = [];
      foreach (var item in typedItems)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

   public List<Location> GetLocations() => LocationChildren.SelectMany(x => x.GetLocations()).ToList();
   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Locations;
   public string GetNamespace => "Map.Region";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RegionAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Region Empty { get; } = new() { UniqueId = "Arcanum_Empty_Region" };

   public LocationCollectionType LcType => LocationCollectionType.Region;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Area> LocationChildren { get; set; } = [];
   // IIndexRandomColor Implementation
   public int Index { get; set; }
}