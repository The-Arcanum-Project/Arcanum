using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Religion;

public partial class ReligiousSchool(string name) : INUI, ICollectionProvider<ReligiousSchool>, IEmpty<ReligiousSchool>
{
   # region Nexus Properties

   [Description("The name of this religious school.")]
   [ReadonlyNexus]
   public string Name { get; set; } = name;
   [Description("How this religious school relates to other religious schools.")]
   public ObservableRangeCollection<KeyValuePair<ReligiousSchool, ReligiousSchoolRelationType>>
      Relations { get; set; } = [];

   # endregion

   public bool IsReadonly => false;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.ReligiousSchoolSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<ReligiousSchool> GetGlobalItems() => Globals.ReligiousSchools.Values;
   public static ReligiousSchool Empty { get; } = new("EmptyArcanum_ReligiousSchool");

   public override bool Equals(object? obj) => obj is ReligiousSchool other &&
                                               string.Equals(Name, other.Name, StringComparison.Ordinal);

   // ReSharper disable once NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => Name.GetHashCode();
   public override string ToString() => Name;
}