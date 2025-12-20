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
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Religious;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Pops;

[NexusConfig]
[ObjectSaveAs]
public partial class PopDefinition : IEu5Object<PopDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("type")]
   [DefaultValue(null)]
   [Description("The type of population this PopDefinition represents.")]
   public PopType PopType { get; set; } = PopType.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("culture")]
   [DefaultValue(null)]
   [Description("The culture associated with this PopDefinition.")]
   public Culture Culture { get; set; } = Culture.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("religion")]
   [DefaultValue("")]
   [Description("The religion associated with this PopDefinition.")]
   public Religion Religion { get; set; } = Religion.Empty;

   [SaveAs(numOfDecimalPlaces: 3)]
   [ParseAs("size")]
   [DefaultValue(0)]
   [Description("The size of the population.")]
   public double Size { get; set; }

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this PopDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = string.Empty;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Pops.{nameof(PopDefinition)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.PopDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.PopDefinitionAgsSettings;
   public static Dictionary<string, PopDefinition> GetGlobalItems() => [];
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static PopDefinition Empty { get; } = new() { UniqueId = "Arcanum_Empty_PopDefinition" };

   #endregion
}