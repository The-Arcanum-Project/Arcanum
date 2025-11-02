using System.ComponentModel;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;

[ObjectSaveAs]
public partial class CurrencyData : INUI, IEmpty<CurrencyData>, IModifierPattern
{
   [DefaultValue("null")]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   public string UniqueId { get; set; } = null!;

   [DefaultValue(1)]
   [Description("The amount of this currency being added or removed.")]
   public object Value { get; set; } = 1;

   [DefaultValue(ModifierType.Float)]
   [Description("The type of modifier this currency data represents.")]
   public ModifierType Type { get; set; } = ModifierType.Float;

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.CurrencyDataSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static CurrencyData Empty { get; } = new() { UniqueId = "Arcanum_empty_currency_data", Value = 0 };

   public override string ToString() => UniqueId;
   protected bool Equals(CurrencyData other) => UniqueId == other.UniqueId && Value.Equals(other.Value);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((CurrencyData)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(UniqueId, Value);
}