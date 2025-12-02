using System.ComponentModel;
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
public partial class Area : IMapInferable, IEu5Object<Area>, ILocation, ILocationCollection<Province>, IIndexRandomColor
{
   public bool IsReadonly { get; } = false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.AreaSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = this.GetFirstParentOfType(LocationCollectionType.Region);
         if (parent != null)
            navigations.Add(new NUINavigation(parent, $"Region: {parent.UniqueId}"));

         if (LocationChildren.Count > 0)
            navigations.Add(null);

         foreach (var location in LocationChildren)
            navigations.Add(new NUINavigation(location, $"Areas: {location.UniqueId}"));

         return navigations.ToArray()!;
      }
   }
   public static Dictionary<string, Area> GetGlobalItems() => Globals.Areas;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                                               .Area)!)
                                                                          .Distinct()
                                                                          .ToList();

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      var typedItems = items.Cast<Area>();
      List<Location> locations = [];
      foreach (var item in typedItems)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

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
   public List<Location> GetLocations() => LocationChildren.SelectMany(p => p.GetLocations()).ToList();

   public LocationCollectionType LcType => LocationCollectionType.Area;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Province> LocationChildren { get; set; } = [];
   // IIndexRandomColor Implementation
   public int Index { get; set; }
}