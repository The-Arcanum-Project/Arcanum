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
public partial class SubContinentGoodsDemand : IEmbeddedEu5Object<SubContinentGoodsDemand>, IIEu5ObjectDemand<SubContinentGoodsDemand, SubContinent>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("-")]
   [DefaultValue(0f)]
   [Description("The demand value for the specified SubContinent.")]
   public float Demand { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(SubContinent))]
   [DefaultValue(null)]
   [Description("The SubContinent this demand data applies to.")]
   public SubContinent WinterType { get; set; } = SubContinent.Empty;

   # endregion

   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.SubContinentGoodsDemandSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SubContinentGoodsDemand;
   [PropertyConfig(true)]
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static SubContinentGoodsDemand Empty { get; } = new() { UniqueId = "SubContinentGoodsDemand_EMPTY" };

   public void SetData(SubContinent value, float amount)
   {
      WinterType = value;
      Demand = amount;
   }

   public void GetData(out IEu5Object value, out float amount)
   {
      value = WinterType;
      amount = Demand;
   }
}