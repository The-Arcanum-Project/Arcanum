#region

using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Nexus.Core.Attributes;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Court.State;

[ObjectSaveAs(savingMethod: "SaveIdentifierStringKvp")]
[NexusConfig]
public partial class RegnalNumber : IEu5Object<RegnalNumber>, IStringKvp
{
   [DefaultValue("")]
   [Description("The key for this key-value pair.")]
   [SuppressAgs]
   public string Key { get; set; } = string.Empty;

   [DefaultValue("")]
   [Description("The value for this key-value pair.")]
   [SuppressAgs]
   public string Value { get; set; } = string.Empty;

#pragma warning disable AGS004
   [Description("Unique key of this RegnalNumber. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.State.{nameof(RegnalNumber)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.RegnalNumberNUISettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RegnalNumber;
   public static Dictionary<string, RegnalNumber> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static RegnalNumber Empty { get; } = new() { UniqueId = "Arcanum_Empty_RegnalNumber" };

   #endregion
}