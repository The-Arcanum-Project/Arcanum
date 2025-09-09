using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Region : LocationCollection<Area>, INUI, ICollectionProvider<Region>, IMapInferable<Region>, IEmpty<Region>
{
   public Region(FileInformation fileInfo, string name, ICollection<Area> provinces) : base(fileInfo, name, provinces)
   {
   }

   public Region(FileInformation fileInfo, string name) : base(fileInfo, name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.Region;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.RegionSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = GetFirstParentOfType(LocationCollectionType.SuperRegion);
         if (parent != Empty)
            navigations.Add(new NUINavigation((INUI)parent, $"SuperRegion: {parent.Name}"));

         if (SubCollection.Count > 0)
            navigations.Add(null);

         foreach (var location in SubCollection)
            navigations.Add(new NUINavigation(location, $"Region: {location.Name}"));

         return navigations.ToArray()!;
      }
   }
   public static IEnumerable<Region> GetGlobalItems() => Globals.Regions.Values;

   public static List<Region> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                             .Select(loc => (Region)loc
                                                                                    .GetFirstParentOfType(LocationCollectionType
                                                                                           .Area))
                                                                             .Distinct()
                                                                             .ToList();

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public new static Region Empty { get; } = new(FileInformation.Empty, "EmptyArcanum_Region");
}