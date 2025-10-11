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
public partial class Continent
   : IMapInferable<Continent>, IEu5Object<Continent>, ILocation, ILocationCollection<SuperRegion>
{
   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ContinentSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Continent> GetGlobalItems() => Globals.Continents;

   public static List<Continent> GetInferredList(IEnumerable<Location> sLocs) => sLocs
     .Select(loc => (Continent)loc
               .GetFirstParentOfType(LocationCollectionType
                                       .Area)!)
     .Distinct()
     .ToList();

   public static List<Location> GetRelevantLocations(IEnumerable<Continent> items)
   {
      List<Location> locations = [];
      foreach (var item in items)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public string GetNamespace => $"Map.{nameof(Continent)}";

   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ContinentAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static Continent Empty => new() { UniqueId = "Arcanum_Empty_Continent" };
   public ICollection<Location> GetLocations() => LocationChildren.SelectMany(sr => sr.GetLocations()).ToList();
   public LocationCollectionType LcType => LocationCollectionType.Continent;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];
   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<SuperRegion> LocationChildren { get; set; } = [];

   public override string ToString() => UniqueId;

   protected bool Equals(Continent other) => UniqueId == other.UniqueId;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((Continent)obj);
   }

   public override int GetHashCode() => UniqueId.GetHashCode();
}