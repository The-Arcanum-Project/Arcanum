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

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs]
public partial class EstateSatisfactionDefinition : IEu5Object<EstateSatisfactionDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("satisfaction")]
   [DefaultValue(0f)]
   [Description("The base satisfaction value provided by this EstateSatisfactionDefinition.")]
   public float Satisfaction { get; set; }

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this EstateSatisfactionDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.GovernmentState.{nameof(EstateSatisfactionDefinition)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EstateSatisfactionDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.EstateSatisfactionDefinitionAgsSettings;
   public static Dictionary<string, EstateSatisfactionDefinition> GetGlobalItems() => [];

   public static EstateSatisfactionDefinition Empty { get; } =
      new() { UniqueId = "Arcanum_Empty_EstateSatisfactionDefinition" };

   public override string ToString() => UniqueId;

   #endregion
}