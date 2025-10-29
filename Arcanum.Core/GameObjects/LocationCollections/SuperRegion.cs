using System.Collections;
using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Common.UI;

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class SuperRegion
   : IMapInferable, IEu5Object<SuperRegion>, ILocation, ILocationCollection<Region>
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

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                          .Select(IEu5Object (loc) => loc
                                                                                 .GetFirstParentOfType(LocationCollectionType
                                                                                        .Area)!)
                                                                          .Distinct()
                                                                          .ToList();

   public List<Location> GetRelevantLocations(IEnumerable items)
   {
      var typedItems = items.Cast<SuperRegion>();
      List<Location> locations = [];
      foreach (var item in typedItems)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public string GetNamespace => "Map.Superregion";

   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SuperRegionAgsSettings;

   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static SuperRegion Empty { get; } = new() { UniqueId = "Arcanum_Empty_SuperRegion" };
   public List<Location> GetLocations() => LocationChildren.SelectMany(r => r.GetLocations()).ToList();
   public LocationCollectionType LcType => LocationCollectionType.SuperRegion;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];

   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Region> LocationChildren { get; set; } = [];
}