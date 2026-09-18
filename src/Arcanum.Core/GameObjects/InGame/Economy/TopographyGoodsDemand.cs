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
public partial class TopographyGoodsDemand : IEmbeddedEu5Object<TopographyGoodsDemand>, IIEu5ObjectDemand<TopographyGoodsDemand, Topography>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0f)]
   [Description("The demand value for the specified topography.")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(Topography))]
   [DefaultValue(null)]
   [Description("The Topography this demand data applies to.")]
   public Topography WinterType { get; set; } = Topography.Empty;

   # endregion

   public void SetData(Topography value, float amount)
   {
      WinterType = value;
      Demand = amount;
   }

   public void GetData(out IEu5Object value, out float amount)
   {
      value = WinterType;
      amount = Demand;
   }

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TopographyGoodsDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.TopographyGoodsDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static TopographyGoodsDemand Empty { get; } = new() { UniqueId = "TopographyGoodsDemand_EMPTY" };
}