using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Common.UI;

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class SuperRegion
   : IMapInferable<SuperRegion>, IEu5Object<SuperRegion>, ILocation, ILocationCollection<Region>
{
   public bool IsReadonly { get; } = false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.SuperRegionSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = this.GetFirstParentOfType(LocationCollectionType.Continent);
         if (parent != null)
            navigations.Add(new NUINavigation(parent, $"Continent: {parent.UniqueId}"));

         if (LocationChildren.Count > 0)
            navigations.Add(null);

         foreach (var location in LocationChildren)
            navigations.Add(new NUINavigation(location, $"Location: {location.UniqueId}"));

         return navigations.ToArray()!;
      }
   }
   public static Dictionary<string, SuperRegion> GetGlobalItems() => Globals.SuperRegions;

   public static List<SuperRegion> GetInferredList(IEnumerable<Location> sLocs) => sLocs
     .Select(loc => (SuperRegion)loc
               .GetFirstParentOfType(LocationCollectionType
                                       .Area)!)
     .Distinct()
     .ToList();

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public string GetNamespace => "Map.Superregion";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SuperRegionAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public static SuperRegion Empty { get; } = new() { UniqueId = "Arcanum_Empty_SuperRegion" };
   public ICollection<Location> GetLocations() => LocationChildren.SelectMany(r => r.GetLocations()).ToList();
   public LocationCollectionType LcType => LocationCollectionType.SuperRegion;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];

   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Region> LocationChildren { get; set; } = [];
}