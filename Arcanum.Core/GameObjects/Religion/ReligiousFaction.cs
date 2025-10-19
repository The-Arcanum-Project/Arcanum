using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Religion;

[ObjectSaveAs]
public partial class ReligiousFaction : IEu5Object<ReligiousFaction>
{
   #region Nexus Properties

   // Only discovered, not parsed.

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this ReligiousFaction. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligiousFaction)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousFactionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousFactionAgsSettings;
   public static Dictionary<string, ReligiousFaction> GetGlobalItems() => Globals.ReligiousFactions;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static ReligiousFaction Empty { get; } = new() { UniqueId = "Arcanum_Empty_ReligiousFaction" };

   public override string ToString() => UniqueId;

   #endregion
}