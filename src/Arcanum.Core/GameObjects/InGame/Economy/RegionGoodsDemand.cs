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
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Economy;

[ObjectSaveAs(savingMethod: "DemandSaving")]
public partial class RegionGoodsDemand : IEmbeddedEu5Object<RegionGoodsDemand>, IIEu5ObjectDemand<RegionGoodsDemand, Region>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0f)]
   [Description("The demand value for the specified Region.")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(Region))]
   [DefaultValue(null)]
   [Description("The Region this demand data applies to.")]
   public Region Region { get; set; } = Region.Empty;

   # endregion

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.RegionGoodsDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RegionGoodsDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static RegionGoodsDemand Empty { get; } = new() { UniqueId = "RegionGoodsDemand_EMPTY" };

   public void SetData(Region value, float amount)
   {
      Region = value;
      Demand = amount;
   }

   public void GetData(out IEu5Object value, out float amount)
   {
      value = Region;
      amount = Demand;
   }
}