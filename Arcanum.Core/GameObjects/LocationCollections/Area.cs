using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Common.UI;

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class Area : IMapInferable<Area>, IEu5Object<Area>, ILocation, ILocationCollection<Province>
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

   public static List<Area> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                           .Select(loc => (Area)
                                                                               loc
                                                                                 .GetFirstParentOfType(LocationCollectionType
                                                                                    .Area)!)
                                                                           .Distinct()
                                                                           .ToList();

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public string GetNamespace => $"Map.{nameof(Area)}";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.AreaAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public static Area Empty { get; } = new() { UniqueId = "Arcanum_Empty_Area" };
   public ICollection<Location> GetLocations() => LocationChildren.SelectMany(p => p.GetLocations()).ToList();

   public LocationCollectionType LcType => LocationCollectionType.Area;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Province> LocationChildren { get; set; } = [];
}