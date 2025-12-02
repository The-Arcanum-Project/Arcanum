using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
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
public partial class SuperRegion
   : IMapInferable, IEu5Object<SuperRegion>, ILocation, ILocationCollection<Region>, IIndexRandomColor
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
                                                                                                               .SuperRegion)!)
                                                                          .Distinct()
                                                                          .ToList();

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      var typedItems = items.Cast<SuperRegion>();
      List<Location> locations = [];
      foreach (var item in typedItems)
         locations.AddRange(item.GetLocations());
      return locations.Distinct().ToList();
   }

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.SuperRegions;
   public string GetNamespace => "Map.Superregion";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.MapObjects |
                                 IQueastorSearchSettings.DefaultCategories.GameObjects;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SuperRegionAgsSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   [Description("Unique key of this SuperRegion. Must be unique among all objects of this type.")]
   [DefaultValue("")]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static SuperRegion Empty { get; } = new() { UniqueId = "Arcanum_Empty_SuperRegion" };
   public List<Location> GetLocations() => LocationChildren.SelectMany(r => r.GetLocations()).ToList();
   public LocationCollectionType LcType => LocationCollectionType.SuperRegion;
   public ObservableRangeCollection<ILocation> Parents { get; set; } = [];

    [ParseAs("null", ignore: true)]
    [Description("The Continent this SuperRegion belongs to.")]
    [DefaultValue(null)]
    [SuppressAgs]
   public Continent Continent
   {
      get => field;

      set
      {
         if (field != Continent.Empty)
            field.SuperRegions._removeFromChild(this);
         if (value != Continent.Empty)
            value.SuperRegions._addFromChild(this);
         
         field = value;
      }
   } = Continent.Empty;

   [SaveAs(isEmbeddedObject: true)]
   public ObservableRangeCollection<Region> LocationChildren { get; set; } = [];
   // IIndexRandomColor Implementation
   public int Index { get; set; }
}