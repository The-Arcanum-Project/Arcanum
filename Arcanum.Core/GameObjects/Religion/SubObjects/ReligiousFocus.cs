using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Religion.SubObjects;

[ObjectSaveAs]
public partial class ReligiousFocus : IEu5Object<ReligiousFocus>
{
   #region Nexus Properties

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this ReligiousFocus. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligiousFocus)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousFocusSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousFocusAgsSettings;
   public static Dictionary<string, ReligiousFocus> GetGlobalItems() => Globals.ReligiousFocuses;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static ReligiousFocus Empty { get; } = new() { UniqueId = "Arcanum_Empty_ReligiousFocus" };

   public override string ToString() => UniqueId;

   #endregion
}