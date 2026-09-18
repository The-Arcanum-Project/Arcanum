#region

using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Map;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Economy;

[ObjectSaveAs(savingMethod: "DemandSaving")]
public partial class ClimateDemand : IEmbeddedEu5Object<ClimateDemand>, IIEu5ObjectDemand<ClimateDemand, Climate>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0f)]
   [Description("The demand value for the specified Region.")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(Climate))]
   [DefaultValue(null)]
   [Description("The climate this demand data applies to.")]
   public Climate Climate { get; set; } = Climate.Empty;

   # endregion

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ClimateDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ClimateDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static ClimateDemand Empty { get; } = new() { UniqueId = "ClimateDemand_EMPTY" };

   public void SetData(Climate value, float amount)
   {
      Climate = value;
      Demand = amount;
   }

   public void GetData(out IEu5Object value, out float amount)
   {
      value = Climate;
      amount = Demand;
   }
}