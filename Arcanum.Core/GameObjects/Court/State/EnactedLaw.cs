using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs(savingMethod: "SaveIdentifierStringKvp")]
#pragma warning disable ARC002
public partial class EnactedLaw : INUI, IAgs, IStringKvp, IEmpty<EnactedLaw>
#pragma warning restore ARC002
{
   [SuppressAgs]
   [DefaultValue("")]
   [Description("The name of the law.")]
   public string Key { get; set; } = string.Empty;

   [SuppressAgs]
   [DefaultValue("")]
   [Description("The value of the enacted law.")]
   public string Value { get; set; } = string.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EnactedLawSettings;
   public INUINavigation[] Navigations => throw new NotImplementedException();
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.EnactedLawAgsSettings;
   public string SavingKey => string.Empty;
   public static EnactedLaw Empty { get; } = new() { Key = string.Empty, Value = string.Empty };
}