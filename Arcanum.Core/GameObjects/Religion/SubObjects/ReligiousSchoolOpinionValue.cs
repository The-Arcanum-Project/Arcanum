using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Culture;

namespace Arcanum.Core.GameObjects.Religion.SubObjects;

[ObjectSaveAs(savingMethod: "SaveIAgsEnumKvp")]
#pragma warning disable ARC002
public partial class ReligiousSchoolOpinionValue
   : IEmpty<ReligiousSchoolOpinionValue>, IIagsEnumKvp<ReligiousSchool, Opinion>
#pragma warning restore ARC002
{
   [SuppressAgs]
   [DefaultValue("")]
   [Description("The culture this opinion is about.")]
   public ReligiousSchool Key { get; set; } = ReligiousSchool.Empty;

   [SuppressAgs]
   [DefaultValue("")]
   [Description("The opinion value.")]
   public Opinion Value { get; set; } = Opinion.Neutral;

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousSchoolOpinionValueSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousSchoolOpinionValueAgsSettings;
   public string SavingKey => string.Empty;
   public static ReligiousSchoolOpinionValue Empty { get; } =
      new() { Key = ReligiousSchool.Empty, Value = Opinion.Neutral };

   #endregion

   #region Equality Members

   protected bool Equals(ReligiousSchoolOpinionValue other) => Key.Equals(other.Key) && Value.Equals(other.Value);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((ReligiousSchoolOpinionValue)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(Key, Value);

   #endregion
}