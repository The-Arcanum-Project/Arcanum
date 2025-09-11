using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.AbstractMechanics;

public partial class Age(string name) : NameKeyDefined(name), INUI, IEmpty<Age>, ICollectionProvider<Age>
{
   # region Nexus Properties

   [ParseAs(AstNodeType.ContentNode, "year")]
   [Description("What year the age starts in.")]
   public int Year { get; set; }

   [ParseAs(AstNodeType.ContentNode, "max_price")]
   [Description("The maximum price for goods during this age.")]
   public int MaxPrice { get; set; }

   [ParseAs(AstNodeType.ContentNode, "price_stability")]
   [Description("A modifier to price stability during this age.")]
   public float PriceStability { get; set; }

   [ParseAs(AstNodeType.ContentNode, "mercenaries")]
   [Description("The factor for the size of mercenary armies during this age.")]
   public float Mercenaries { get; set; } = 1f;

   [ParseAs(AstNodeType.ContentNode, "hegemons_allowed")]
   [Description("Whether hegemons are allowed to be formed during this age.")]
   public bool AllowHegemons { get; set; } = false;

   [ParseAs(AstNodeType.ContentNode, "efficiency")]
   [Description("Some economic efficiency modifier during this age.")]
   public float Efficiency { get; set; } = 1f;

   [ParseAs(AstNodeType.ContentNode, "war_score_from_battles")]
   [Description("The factor for war score gained from battles during this age.")]
   public float WarScoreFromBattles { get; set; } = 1f;

   # endregion

   #region Interface Properties

   public bool IsReadonly => true;
   public NUISetting Settings => Config.Settings.NUIObjectSettings.AgeSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Age Empty { get; } = new("Arcanum_Empty_Age");
   public static IEnumerable<Age> GetGlobalItems() => Globals.Ages;

   #endregion
}