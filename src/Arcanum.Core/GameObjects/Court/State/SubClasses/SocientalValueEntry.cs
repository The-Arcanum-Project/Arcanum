using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.GameObjects.Court.State.SubClasses;

[ObjectSaveAs]
public partial class SocientalValueEntry : IEmbeddedEu5Object<SocientalValueEntry>
{
   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(SocientalValue))]
   [DefaultValue(null)]
   [Description("The sociental value associated with this entry.")]
   public SocientalValue SocientalValue { get; set; } = SocientalValue.Empty;

   [SaveAs]
   [ParseAs(Globals.DO_NOT_PARSE_ME)]
   [Description("The value assigned to the sociental value.")]
   [DefaultValue(0)]
   public int Value { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.SocientalValueEntrySettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SocientalValueEntryAgsSettings;
   [PropertyConfig(isReadonly: true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static SocientalValueEntry Empty { get; } = new () { UniqueId = "SocientalValueEntry_EMPTY" };
}