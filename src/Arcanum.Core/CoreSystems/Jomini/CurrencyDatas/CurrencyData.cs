using System.ComponentModel;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core.Attributes;

namespace Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;

[NexusConfig]
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
}