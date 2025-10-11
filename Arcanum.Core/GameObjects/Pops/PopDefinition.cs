using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Pops;

[ObjectSaveAs]
public partial class PopDefinition : IEu5Object<PopDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("type")]
   [DefaultValue(null)]
   [Description("The type of population this PopDefinition represents.")]
   public PopType PopType { get; set; } = PopType.Empty;

   [SaveAs]
   [ParseAs("culture")]
   [DefaultValue(null)]
   [Description("The culture associated with this PopDefinition.")]
   public Culture.Culture Culture { get; set; } = GameObjects.Culture.Culture.Empty;

   [SaveAs]
   [ParseAs("religion")]
   [DefaultValue("")]
   [Description("The religion associated with this PopDefinition.")]
   public string Religion { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("size")]
   [DefaultValue(0f)]
   [Description("The size of the population.")]
   public float Size { get; set; }

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this PopDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Pops.{nameof(PopDefinition)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.PopDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.PopDefinitionAgsSettings;
   public static Dictionary<string, PopDefinition> GetGlobalItems() => [];

   public static PopDefinition Empty { get; } = new() { UniqueId = "Arcanum_Empty_PopDefinition" };

   public override string ToString() => UniqueId;

   #endregion
}