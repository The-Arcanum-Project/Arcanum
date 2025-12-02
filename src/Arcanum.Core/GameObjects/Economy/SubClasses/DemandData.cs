using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.Pops;

namespace Arcanum.Core.GameObjects.Economy.SubClasses;

[ObjectSaveAs(savingMethod: "SaveDemandData")]
public partial class DemandData : IEmbeddedEu5Object<DemandData>
{
   [SaveAs]
   [ParseAs("all")]
   [DefaultValue(0.0f)]
   [Description("The demand value for the specified estate(s).")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", ignore: true, iEu5KeyType: typeof(PopType))]
   [DefaultValue(null)]
   [Description("The estate this demand data applies to.")]
   public PopType PopType { get; set; } = PopType.Empty;

   [SaveAs]
   [ParseAs("upper")]
   [DefaultValue(0f)]
   [Description("Whether this demand data applies to all estates.")]
   public float TargetUpper { get; set; }
   [SaveAs]
   [ParseAs("all")]
   [DefaultValue(0f)]
   [Description("Whether this demand data applies to all estates.")]
   public float TargetAll { get; set; }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.DemandDataSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.DemandDataAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public static DemandData Empty { get; } = new();
}