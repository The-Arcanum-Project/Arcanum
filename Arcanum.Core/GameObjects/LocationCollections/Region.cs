using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Region : LocationCollection<Area>, INUI
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
   public NUISetting Settings { get; } = Config.Settings.NUISettings.RegionSettings;
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
}