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
using Arcanum.Core.GameObjects.InGame.Pops;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Economy.SubClasses;

[ObjectSaveAs(savingMethod: "SaveWealthImpactData")]
[NexusConfig]
public partial class WealthImpactData : IEu5Object<WealthImpactData>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("all")]
   [DefaultValue(0.0f)]
   [Description("The demand value for the specified estate(s).")]
   public float WealthImpact { get; set; }

   [SaveAs]
   [ParseAs("-", iEu5KeyType: typeof(PopType))]
   [DefaultValue(null)]
   [Description("The estate this demand data applies to.")]
   public PopType PopType { get; set; } = PopType.Empty;

   [SaveAs]
   [ParseAs("all")]
   [DefaultValue(0f)]
   [Description("Whether this demand data applies to all estates.")]
   public float TargetAll { get; set; }

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this WealthImpactData. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Economy.{nameof(WealthImpactData)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.WealthImpactDataSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.WealthImpactData;
   public static Dictionary<string, WealthImpactData> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static WealthImpactData Empty { get; } = new() { UniqueId = "Arcanum_Empty_WealthImpactData" };

   #endregion
}