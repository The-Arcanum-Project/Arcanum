using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.GameObjects.InGame.Court.State;

[ObjectSaveAs(savingMethod: "SaveIdentifierStringKvp")]
#pragma warning disable ARC002
public partial class RegnalNumber : INUI, IAgs, IStringKvp, IEmpty<RegnalNumber>
#pragma warning restore ARC002
{
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.RegnalNumberNUISettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RegnalNumberAgsSettings;
   public string SavingKey => string.Empty;

   [DefaultValue("")]
   [Description("The key for this key-value pair.")]
   public string Key { get; set; } = string.Empty;

   [DefaultValue("")]
   [Description("The value for this key-value pair.")]
   public string Value { get; set; } = string.Empty;
   public static RegnalNumber Empty { get; } = new() { Key = string.Empty, Value = string.Empty };
}