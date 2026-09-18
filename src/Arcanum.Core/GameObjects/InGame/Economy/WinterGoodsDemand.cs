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
public partial class WinterGoodsDemand : IEmbeddedEu5Object<WinterGoodsDemand>, IEnumDemand<WinterGoodsDemand, Climate.WinterType>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0f)]
   [Description("The demand value for the specified WinterType.")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(Climate.WinterType))]
   [DefaultValue(null)]
   [Description("The WinterType this demand data applies to.")]
   public Climate.WinterType WinterType { get; set; } = Climate.WinterType.None;

   # endregion

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.WinterGoodsDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.WinterGoodsDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static WinterGoodsDemand Empty { get; } = new() { UniqueId = "WinterGoodsDemand_EMPTY" };

   public void SetData(Climate.WinterType value, float amount)
   {
      WinterType = value;
      Demand = amount;
   }

   public void GetData(out Enum value, out float amount)
   {
      value = WinterType;
      amount = Demand;
   }
}