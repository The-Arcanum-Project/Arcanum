using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class SuperRegion : LocationCollection<Region>, INUI, ICollectionProvider<SuperRegion>
{
   public SuperRegion(FileInformation fileInfo, string name, ICollection<Region> provinces) : base(fileInfo, name, provinces)
   {
   }

   public SuperRegion(FileInformation fileInfo, string name) : base(fileInfo, name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.SuperRegion;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.SuperRegionSettings;
   public INUINavigation[] Navigations 
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = GetFirstParentOfType(LocationCollectionType.Continent);
         if (parent != Empty)
            navigations.Add(new NUINavigation((INUI)parent, $"Continent: {parent.Name}"));
         
         if (SubCollection.Count > 0)
            navigations.Add(null);
         
         foreach (var location in SubCollection)
            navigations.Add(new NUINavigation(location, $"Location: {location.Name}"));
         
         return navigations.ToArray()!;
      }
   }
   public static IEnumerable<SuperRegion> GetGlobalItems() => Globals.SuperRegions.Values;
}