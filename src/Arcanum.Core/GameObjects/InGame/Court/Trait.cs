using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Court;

[NexusConfig]
[ObjectSaveAs]
public partial class Trait : IEu5Object<Trait>
{
   #region Nexus Properties

   [ParseAs("category")]
   [DefaultValue(TraitCategory.Ruler)]
   [SaveAs(SavingValueType.Identifier)]
   [Description("Category of this Trait.")]
   public TraitCategory Category { get; set; } = TraitCategory.Ruler;

   [ParseAs("flavor")]
   [DefaultValue(TraitFlavorType.None)]
   [SaveAs(SavingValueType.Identifier)]
   [Description("Flavor type of this Trait.")]
   public TraitFlavorType Flavor { get; set; } = TraitFlavorType.None;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to this trait.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Trait. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.Character.{nameof(Trait)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TraitSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.TraitAgsSettings;
   public static Dictionary<string, Trait> GetGlobalItems() => Globals.Traits;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static Trait Empty { get; } = new() { UniqueId = "Arcanum_Empty_Trait" };

   #endregion
}