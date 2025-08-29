using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class Tag(string name) : INUI, ICollectionProvider<Tag>
{
   public string Name { get; set; } = name;

   public static Tag Empty { get; } = new(string.Empty);

   public override string ToString() => Name;

   public static IEnumerable<Tag> GetGlobalItems() => Globals.Countries.Keys;

   public override bool Equals(object? obj)
   {
      if (obj is Tag other)
         return Name == other.Name;

      return false;
   }

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();
   
   public bool IsValid => Name.Length == 3;
   public bool IsReadonly => true;
   public NUISetting Settings { get; } = Config.Settings.NUIObjectSettings.TagSettings;
   public INUINavigation[] Navigations { get; } = [];
}