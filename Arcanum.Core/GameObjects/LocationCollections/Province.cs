using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Province : LocationCollection<Location>, INUI, ICollectionProvider<Province>
{
   public Province(FileInformation fileInfo, string name, ICollection<Location> provinces) :
      base(fileInfo, name, provinces)
   {
   }

   public Province(FileInformation fileInfo, string name) : base(fileInfo, name)
   {
   }

   public override LocationCollectionType LCType => LocationCollectionType.Province;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.ProvinceSettings;
   public INUINavigation[] Navigations
   {
      get
      {
         List<INUINavigation?> navigations = [];
         var parent = GetFirstParentOfType(LocationCollectionType.Area);
         if (parent != Empty)
            navigations.Add(new NUINavigation((INUI)parent, $"Area: {parent.Name}"));
         
         if (SubCollection.Count > 0)
            navigations.Add(null);
         
         foreach (var location in SubCollection)
            navigations.Add(new NUINavigation(location, $"Location: {location.Name}"));
         
         return navigations.ToArray()!;
      }
   }
   public static IEnumerable<Province> GetGlobalItems() => Globals.Provinces.Values;
}