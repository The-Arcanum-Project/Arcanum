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
public partial class ContinentDemand : IEmbeddedEu5Object<ContinentDemand>, IIEu5ObjectDemand<ContinentDemand, Continent>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0f)]
   [Description("The demand value for the specified Region.")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(Continent))]
   [DefaultValue(null)]
   [Description("The continent this demand data applies to.")]
   public Continent Continent { get; set; } = Continent.Empty;

   # endregion

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ContinenteDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ContinentDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static ContinentDemand Empty { get; } = new() { UniqueId = "ContinenteDemand_EMPTY" };

   public void SetData(Continent value, float amount)
   {
      Continent = value;
      Demand = amount;
   }

   public void GetData(out IEu5Object value, out float amount)
   {
      value = Continent;
      amount = Demand;
   }
}