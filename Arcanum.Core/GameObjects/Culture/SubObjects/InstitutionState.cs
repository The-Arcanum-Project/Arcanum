using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Common.UI;

namespace Arcanum.Core.GameObjects.Culture.SubObjects;

[ObjectSaveAs]
public partial class InstitutionState : IEu5Object<InstitutionState>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("active")]
   [Description("Whether this InstitutionState is currently active.")]
   public bool IsActive { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("birth_place")]
   [Description("The location where this InstitutionState was founded.")]
   public Location BirthPlace { get; set; } = Location.Empty;

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

   public string GetNamespace => $"MainMenu.State.{nameof(InstitutionState)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.InstitutionStateSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.InstitutionStateAgsSettings;

   public static Dictionary<string, InstitutionState> GetGlobalItems()
      => Globals.State.InstitutionManager.InstitutionStates.ToDictionary(i => i.UniqueId, i => i);

   public static InstitutionState Empty { get; } = new() { UniqueId = "Arcanum_Empty_InstitutionState" };

   public override string ToString() => UniqueId;

   #endregion
}