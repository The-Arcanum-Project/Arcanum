using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Culture.SubObjects;
using Arcanum.Core.GlobalStates;
using Common.UI;

namespace Arcanum.Core.GameObjects.MainMenu.States;

[ObjectSaveAs]
public partial class InstitutionManager : IEu5Object<InstitutionManager>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("institutions", isEmbedded: true)]
   [Description("List of all defined InstitutionStates in the game.")]
   public ObservableRangeCollection<InstitutionState> InstitutionStates { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this InstitutionState. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string SavingKey => "institution_manager";
   public string GetNamespace => $"MainMenu.State.{nameof(InstitutionManager)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory => IQueastorSearchSettings.Category.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.InstitutionManagerSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.InstitutionStateAgsSettings;

   public static Dictionary<string, InstitutionManager> GetGlobalItems()
      => new() { { "State", Globals.State.InstitutionManager } };

   public static InstitutionManager Empty { get; } = new() { UniqueId = "Arcanum_Empty_InstitutionState" };

   public override string ToString() => UniqueId;

   #endregion
}