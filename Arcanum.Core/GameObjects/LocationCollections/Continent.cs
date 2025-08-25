using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;
using Arcanum.Core.GameObjects.LocationCollections.BaseClasses;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Continent : LocationCollection<SuperRegion>, INUI, ICollectionProvider<Continent>
{
   public Continent(FileInformation fileInfo, string name, ICollection<SuperRegion> provinces) : base(fileInfo, name, provinces)
   {
   }

   public Continent(FileInformation fileInfo, string name) : base(fileInfo, name)
   {
   }
   public override LocationCollectionType LCType => LocationCollectionType.Continent;

   public override void RemoveGlobal()
   {
      throw new NotImplementedException();
   }

   public override void AddGlobal()
   {
      throw new NotImplementedException();
   }

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.ContinentSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<Continent> GetGlobalItems() => Globals.Continents.Values;
}