using System.ComponentModel;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.LocationCollections;

public partial class LocationRank(string name, int order)
   : OldNameKeyDefined(name), INUI, ICollectionProvider<LocationRank>, IEmpty<LocationRank>
{
   [Description("The color associated with this location rank, often used in the UI.")]
   [ParseAs("color", AstNodeType.ContentNode)]
   public JominiColor ColorKey { get; set; } = JominiColor.Empty;
   [Description("The rank of the location rank.")]
   [ParseAs("max_rank", AstNodeType.ContentNode)]
   public bool IsMaxRank { get; set; }
   [Description("Whether this location rank is considered an established city.")]
   [ParseAs("is_established_city", AstNodeType.ContentNode)]
   public bool IsEstablishedCity { get; set; }
   [Description("Whether this location rank should be shown in labels.")]
   [ParseAs("show_in_label", AstNodeType.ContentNode)]
   public bool ShowInLabel { get; set; }
   [Description("The number of days it takes to build this location rank.")]
   [ParseAs("build_time", AstNodeType.ContentNode)]
   public int BuildTime { get; set; }
   [Description("The tier of the frame used for this location rank.")]
   [ParseAs("frame_tier", AstNodeType.ContentNode)]
   public int FrameTier { get; set; }
   [Description("What type of construction is required to build this location rank.")]
   [ParseAs("construction_demand", AstNodeType.ContentNode)]
   public string ConstructionDemand { get; set; } = string.Empty;

   /// <summary>
   /// LocationRanks are ordered by appearance in the file, meaning the first one is the best and everything beneath is worse.
   /// </summary>
   public int Order { get; set; } = order;

   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.LocationRankSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static IEnumerable<LocationRank> GetGlobalItems() => Globals.LocationRanks;
   public static LocationRank Empty { get; } = new("empty", int.MinValue);

   #region ISearchable

   public override string GetNamespace => nameof(LocationRank);

   #endregion
}