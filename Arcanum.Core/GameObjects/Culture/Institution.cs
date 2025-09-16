using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Culture;

public partial class Institution(string name) : INUI, IEmpty<Institution>, ICollectionProvider<Institution>
{
   # region Nexus Properties

   [Description("The name of this institution.")]
   [ReadonlyNexus]
   public string Name { get; set; } = name;
   [Description("Whether this institution is currently active.")]
   public bool IsActive { get; set; }
   [Description("The location where this institution was founded.")]
   public Location BirthPlace { get; set; } = Location.Empty;
   [Description("The age in which this institution was founded.")]
   public string Age { get; set; } = string.Empty;

   # endregion

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.InstitutionSettings;
   public INUINavigation[] Navigations => [new NUINavigation(BirthPlace, "Go to Birth Place")];
   public static Institution Empty { get; } = new("Arcanum_Empty_Institution");
   public static IEnumerable<Institution> GetGlobalItems() => Globals.Institutions.Values;

   public override bool Equals(object? obj) => obj is Institution other &&
                                               string.Equals(Name, other.Name, StringComparison.Ordinal);

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();

   public override string ToString() => Name;
}