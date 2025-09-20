using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GameObjects.Court.State;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(GovernmentState),
             ignoredContentKeys:
             [
                "centralization_vs_decentralization", "aristocracy_vs_plutocracy", "serfdom_vs_free_subjects",
                "traditionalist_vs_innovative", "spiritualist_vs_humanist", "mercantilism_vs_free_trade",
                "offensive_vs_defensive", "land_vs_naval", "quality_vs_quantity", "belligerent_vs_conciliatory",
                "capital_economy_vs_traditional_economy", "individualism_vs_communalism", "outward_vs_inward",
                "mysticism_vs_jurisprudence",
             ],
             ignoredBlockKeys: ["variables"])]
public static partial class GovernmentsStateParsing;