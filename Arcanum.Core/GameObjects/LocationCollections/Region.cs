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
public partial class Region : IMapInferable<Region>, IEu5Object<Region>, ILocation, ILocationCollection<Area>
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

   public static List<Region> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                             .Select(loc => (Region)loc
                                                                                .GetFirstParentOfType(LocationCollectionType
                                                                                   .Area)!)
                                                                             .Distinct()
                                                                             .ToList();

   public ICollection<Location> GetLocations() => LocationChildren.SelectMany(x => x.GetLocations()).ToList();
   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public string GetNamespace => "Map.Region";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RegionAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public static Region Empty { get; } = new() { UniqueId = "Arcanum_Empty_Region" };

   public LocationCollectionType LcType => LocationCollectionType.Region;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Area> LocationChildren { get; set; } = [];
}