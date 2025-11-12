using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court.State.SubClasses;

[ObjectSaveAs]
public partial class DesignateHeirReason : IEu5Object<DesignateHeirReason>
{
   #region Nexus Properties

   // There is literally nothing in the files lol

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this DesignateHeirReason. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.Government.{nameof(DesignateHeirReason)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.DesignateHeirReasonSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.DesignateHeirReasonAgsSettings;
   public static Dictionary<string, DesignateHeirReason> GetGlobalItems() => Globals.DesignateHeirReasons;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static DesignateHeirReason Empty { get; } = new() { UniqueId = "Arcanum_Empty_DesignateHeirReason" };

   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public override string ToString() => UniqueId;

   #endregion
}