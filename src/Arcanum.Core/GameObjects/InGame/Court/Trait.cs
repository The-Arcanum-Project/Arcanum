#region

using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;
using Nexus.Core.Attributes;

#endregion

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
   [DefaultValue(false)]
   [ParseAs("is_bad")]
   [Description("Indicates whether this Trait is considered bad.")]
   public bool IsBad { get; set; }

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("yearly_chance_of_remove")]
   [Description("The yearly chance of this trait being removed.")]
   public float YearlyChanceOfRemove { get; set; }

   [SaveAs(numOfDecimalPlaces: 4)]
   [DefaultValue(0f)]
   [ParseAs("chance_on_birth")]
   [Description("The chance of this trait being assigned at birth.")]
   public float ChanceOnBirth { get; set; }

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("chance_after_battle")]
   [Description("The chance of this trait being assigned after a battle.")]
   public float ChanceAfterBattle { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("max_number_of_birth_siblings")]
   [Description("The maximum number of sibling characters that can have this trait at birth.")]
   public int MaxNumberOfBirthSiblings { get; set; }

   [SaveAs]
   [DefaultValue(0f)]
   [ParseAs("yearly_chance_to_die")]
   [Description("The yearly chance of this trait causing the character to die.")]
   public float YearlyChanceToDie { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("upgrades_to")]
   [Description("The trait that this trait can upgrade to.")]
   public Trait UpgradeTo { get; set; } = Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("recovery_trait")]
   [Description("The trait that this trait can recover to.")]
   public Trait RecoveryTrait { get; set; } = Empty;

   [SaveAs]
   [DefaultValue(JuvenileForm.None)]
   [ParseAs("juvenile_form")]
   [Description("The juvenile form associated with this trait.")]
   public JuvenileForm JuvenileForm { get; set; } = JuvenileForm.None;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to this trait.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("custom_tags", itemNodeType: AstNodeType.KeyOnlyNode)]
   [Description("Custom tags applied to this trait.")]
   public ObservableRangeCollection<string> CustomTags { get; set; } = [];

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
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TraitSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.Trait;
   public static Dictionary<string, Trait> GetGlobalItems() => Globals.Traits;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static Trait Empty { get; } = new() { UniqueId = "Arcanum_Empty_Trait" };

   #endregion
}