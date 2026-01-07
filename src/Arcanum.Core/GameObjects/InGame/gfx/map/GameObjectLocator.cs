using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.gfx.map;

[ObjectSaveAs]
[NexusConfig]
public partial class GameObjectLocator : IEu5Object<GameObjectLocator>
{
   #region Nexus Properties

   [Description("The name of objects this locator holds.")]
   [DefaultValue("")]
   [ParseAs("name")]
   [SaveAs(alwaysWrite: true)]
   public string Name { get; set; } = string.Empty;

   [Description("Whether this locator is clamped to the water level.")]
   [DefaultValue(false)]
   [ParseAs("clamp_to_water_level")]
   [SaveAs(alwaysWrite: true)]
   public bool ClampToWaterLevel { get; set; }

   [Description("Whether this locator renders underwater.")]
   [DefaultValue(false)]
   [ParseAs("render_under_water")]
   [SaveAs(alwaysWrite: true)]
   public bool RenderUnderwater { get; set; }

   [Description("Whether this locator holds generated content.")]
   [DefaultValue(false)]
   [ParseAs("generated_content")]
   [SaveAs(alwaysWrite: true)]
   public bool GeneratedContent { get; set; }

   [Description("The layers the locator belongs to.")]
   [DefaultValue("")]
   [ParseAs("layer")]
   [SaveAs(alwaysWrite: true)]
   public string Layer { get; set; } = string.Empty;

   [Description("The nudge data associated with this locator.")]
   [DefaultValue(null)]
   [ParseAs("instances", itemNodeType: AstNodeType.BlockNode, isArray: true)]
   [SaveAs(alwaysWrite: true)]
   public ObservableHashSet<NudgeData> NudgeDatas { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this GameObjectLocator. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId
   {
      get => Name;
      set;
   }

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Gfx.Map.{nameof(GameObjectLocator)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.GameObjectLocatorSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.GameObjectLocator;
   public static Dictionary<string, GameObjectLocator> GetGlobalItems() => Globals.GameObjectLocators;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public string SavingKey => "game_object_locator";

   public static GameObjectLocator Empty { get; } = new() { UniqueId = "Arcanum_Empty_GameObjectLocator" };

   #endregion
}