using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.LocationCollections;

[ObjectSaveAs]
public partial class LocationRank : IEu5Object<LocationRank>
{
   # region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("color")]
   [Description("The color of this LocationRank.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("max_rank")]
   [Description("If true, this LocationRank is the maximum rank achievable.")]
   public bool IsMaxRank { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("is_established_city")]
   [Description("If true, locations with this rank are considered established cities.")]
   public bool IsEstablishedCity { get; set; }

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("show_in_label")]
   [Description("If true, this LocationRank will be shown in location labels.")]
   public bool ShowInLabel { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("build_time")]
   [Description("The time in months required to build up to this LocationRank.")]
   public int BuildTime { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("frame_tier")]
   [Description("The frame tier associated with this LocationRank.")]
   public int FrameTier { get; set; }

   [SaveAs(SavingValueType.Identifier)]
   [DefaultValue("")]
   [ParseAs("construction_demand")]
   [Description("The construction demand associated with this LocationRank.")]
   public string ConstructionDemand { get; set; } = string.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("rank_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to locations of this rank.")]
   public ObservableRangeCollection<ModValInstance> RankModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("country_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to the country owning a location of this rank.")]
   public ObservableRangeCollection<ModValInstance> CountryModifiers { get; set; } = [];

   # endregion

#pragma warning disable AGS004
   [Description("Unique key of this LocationRank. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.{nameof(LocationRank)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.LocationRankSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.LocationRankAgsSettings;
   public static Dictionary<string, LocationRank> GetGlobalItems() => Globals.LocationRanks;

   public static LocationRank Empty { get; } = new() { UniqueId = "Arcanum_Empty_LocationRank" };

   #endregion
}