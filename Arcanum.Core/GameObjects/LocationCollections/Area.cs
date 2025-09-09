using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.Map.MapModes.MapModeImplementations;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Area : LocationCollection<Province>, INUI, ICollectionProvider<Area>, IMapInferable<Area>, IEmpty<Area>
{
   public Area(FileInformation fileInfo, string name, ICollection<Province> provinces) : base(fileInfo, name, provinces)
   {
   }

   public Area(FileInformation fileInfo, string name) : base(fileInfo, name)
   {
   }

   public override LocationCollectionType LCType { get; } = LocationCollectionType.Area;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.AreaSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = GetFirstParentOfType(LocationCollectionType.Region);
         if (parent != Empty)
            navigations.Add(new NUINavigation((INUI)parent, $"Region: {parent.Name}"));

         if (SubCollection.Count > 0)
            navigations.Add(null);

         foreach (var location in SubCollection)
            navigations.Add(new NUINavigation(location, $"Areas: {location.Name}"));

         return navigations.ToArray()!;
      }
   }
   public static IEnumerable<Area> GetGlobalItems() => Globals.Areas.Values;

   public static List<Area> GetInferredList(IEnumerable<Location> sLocs) => sLocs
                                                                           .Select(loc => (Area)
                                                                               loc
                                                                                 .GetFirstParentOfType(LocationCollectionType
                                                                                    .Area))
                                                                           .Distinct()
                                                                           .ToList();

   public static IMapMode GetMapMode { get; } = new BaseMapMode();
   public new static Area Empty { get; } = new (FileInformation.Empty, "Empty_Area");
}