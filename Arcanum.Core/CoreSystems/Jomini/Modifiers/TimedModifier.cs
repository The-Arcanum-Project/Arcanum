using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Date;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Common.UI;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

[ObjectSaveAs]
public partial class TimedModifier : IEu5Object<TimedModifier>
{
#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;
   public FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   [SaveAs]
   [ParseAs("modifier")]
   [DefaultValue("")]
   [Description("The unique id of the static modifier to apply.")]
   public string StaticModifierId { get; set; } = string.Empty;

   [SaveAs]
   [ParseAs("start_date")]
   [DefaultValue(typeof(JominiDate), "0.0.0")]
   [Description("The date at which this modifier starts to apply.")]
   public JominiDate StartDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [ParseAs("date")]
   [DefaultValue(typeof(JominiDate), "0.0.0")]
   [Description("The date at which this modifier stops applying.")]
   public JominiDate EndDate { get; set; } = JominiDate.Empty;

   [SaveAs]
   [ParseAs("size")]
   [DefaultValue(1f)]
   [Description("The factor by which the modifier's effects are scaled.")]
   public float SizeFactor { get; set; } = 1f;

   public string GetNamespace => $"Jomini.Modifiers.{nameof(TimedModifier)}";
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings { get; } = Config.Settings.NUIObjectSettings.TimedModifierSettings;
   public INUINavigation[] Navigations { get; } = [];
   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.TimedModifierAgsSettings;
   public string SavingKey => UniqueId;
   public static IEnumerable<TimedModifier> GetGlobalItems() => []; // TODO parse static modifiers

   public static TimedModifier Empty { get; } = new() { UniqueId = "Arcanum_empty_timed_modifier" };
}