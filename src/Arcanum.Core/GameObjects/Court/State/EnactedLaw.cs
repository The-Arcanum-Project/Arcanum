using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court.State;

[ObjectSaveAs(savingMethod: "SaveIdentifierStringKvp")]
#pragma warning disable ARC002
public partial class EnactedLaw : IEu5Object<EnactedLaw>
#pragma warning restore ARC002
{
   [SuppressAgs]
   [DefaultValue("")]
   [Description("The name of the law.")]
   public string Key { get; set; } = string.Empty;

   [SuppressAgs]
   [DefaultValue("")]
   [Description("The value of the enacted law.")]
   public string Value { get; set; } = string.Empty;

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.EnactedLawSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.EnactedLawAgsSettings;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string SavingKey => string.Empty;
   public static EnactedLaw Empty { get; } = new() { Key = string.Empty, Value = string.Empty };
   public string GetNamespace => $"Court.GovernmentState.{nameof(EnactedLaw)}";

   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, Key, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static Dictionary<string, EnactedLaw> GetGlobalItems() => [];
}