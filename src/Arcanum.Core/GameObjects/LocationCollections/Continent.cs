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

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class Continent
   : IMapInferable, IEu5Object<Continent>, ILocation, ILocationCollection<SuperRegion>, IIndexRandomColor
{
   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ContinentSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Dictionary<string, Continent> GetGlobalItems() => Globals.Continents;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                 .GetFirstParentOfType(LocationCollectionType
                                                                                        .Area)!)
                                                                          .Distinct()
                                                                          .ToList();

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      var typedItems = items.Cast<Continent>();
      List<Location> locations = [];
      foreach (var item in typedItems)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Locations;
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
   public static Continent Empty => new() { UniqueId = "Arcanum_Empty_Continent" };
   public List<Location> GetLocations() => LocationChildren.SelectMany(sr => sr.GetLocations()).ToList();
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
   // IIndexRandomColor Implementation
   public int Index { get; set; }
}