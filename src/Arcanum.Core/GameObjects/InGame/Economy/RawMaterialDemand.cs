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

#endregion

namespace Arcanum.Core.GameObjects.InGame.Economy;

[ObjectSaveAs(savingMethod: "DemandSaving")]
public partial class GoodsDemand : IEmbeddedEu5Object<GoodsDemand>, IIEu5ObjectDemand<GoodsDemand, RawMaterial>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0)]
   [Description("The demand value for the specified estate(s).")]
   public int Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(RawMaterial))]
   [DefaultValue(null)]
   [Description("The RawMaterial this demand data applies to.")]
   public RawMaterial RawMaterial { get; set; } = RawMaterial.Empty;

   # endregion

   public void SetData(RawMaterial value, float amount)
   {
      RawMaterial = value;
      Demand = (int)amount;
   }

   public void GetData(out IEu5Object value, out float amount)
   {
      value = RawMaterial;
      amount = Demand;
   }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.GoodsDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.GoodsDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static GoodsDemand Empty { get; } = new() { UniqueId = "GoodsDemand_EMPTY" };
}