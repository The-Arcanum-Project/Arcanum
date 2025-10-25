using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;
using Nexus.Core;

namespace Arcanum.Core.GameObjects.Common;

[ObjectSaveAs]
public partial class StaticModifier : IEu5Object<StaticModifier>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("game_data", AstNodeType.BlockNode, isEmbedded: true)]
   [Description("Game data associated with this modifier.")]
   public ModifierGameData GameData { get; set; } = ModifierGameData.Empty;

   // This is a very special case where we want to trigger a dynamic parser of an inlines object list.
   [ParseAs("-",
              iEu5KeyType: typeof(ModValInstance),
              isShatteredList: true,
              nodeType: AstNodeType.ContentNode,
              itemNodeType: AstNodeType.ContentNode,
              customGlobalsSource: typeof(ModifierDefinition))]
   [Description("Collection of modifiers applied by this StaticModifier.")]
   [SaveAs]
   [DefaultValue(null)]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this StaticModifier. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Modifiers.{nameof(StaticModifier)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.StaticModifierSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.StaticModifierAgsSettings;
   public static Dictionary<string, StaticModifier> GetGlobalItems() => Globals.StaticModifiers;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static StaticModifier Empty { get; } = new() { UniqueId = "Arcanum_Empty_StaticModifier" };

   public override string ToString() => UniqueId;

   #endregion
}