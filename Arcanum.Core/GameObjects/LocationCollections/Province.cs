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
public partial class Province : IMapInferable<Province>, IEu5Object<Province>, ILocation, ILocationCollection<Location>
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

   static List<Province> IMapInferable<Province>.GetInferredList(IEnumerable<Location> sLocs) => sLocs
     .Select(loc => (Province)loc.GetFirstParentOfType(LocationCollectionType.Province)!)
     .Distinct()
     .ToList();

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public static Province Empty { get; } = new() { UniqueId = "Empty Province" };
   public string GetNamespace => "Map.Province";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ProvinceAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public ICollection<Location> GetLocations() => LocationChildren;

   public LocationCollectionType LcType => LocationCollectionType.Province;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(collectionAsPureIdentifierList: true)]
   public ObservableRangeCollection<Location> LocationChildren { get; set; } = [];
}