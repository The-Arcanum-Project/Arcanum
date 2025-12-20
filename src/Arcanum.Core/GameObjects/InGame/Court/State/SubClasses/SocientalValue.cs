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
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;

[ObjectSaveAs]
[NexusConfig]
public partial class SocientalValue : IEu5Object<SocientalValue>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("left_modifier", itemNodeType: AstNodeType.ContentNode, nodeType: AstNodeType.BlockNode)]
   [Description("The modifier applied when the sociental value is on the left side of the spectrum.")]
   public ObservableRangeCollection<ModValInstance> LeftModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("right_modifier", itemNodeType: AstNodeType.ContentNode, nodeType: AstNodeType.BlockNode)]
   [Description("The modifier applied when the sociental value is on the right side of the spectrum.")]
   public ObservableRangeCollection<ModValInstance> RightModifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this SocientalValue. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Government.{nameof(SocientalValue)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.SocientalValueSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.SocientalValueAgsSettings;
   public static Dictionary<string, SocientalValue> GetGlobalItems() => Globals.SocientalValues;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static SocientalValue Empty { get; } = new() { UniqueId = "Arcanum_Empty_SocientalValue" };

   #endregion
}