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
public partial class ReligiousSchool : IEu5Object<ReligiousSchool>
{
   #region Nexus Properties

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this ReligiousSchool. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligiousSchool)}";
   public void OnSearchSelected() => UIHandle.Instance.MainWindowsHandle.SetToNui(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousSchoolSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousSchoolAgsSettings;
   public static Dictionary<string, ReligiousSchool> GetGlobalItems() => Globals.ReligiousSchools;

   public static ReligiousSchool Empty { get; } = new() { UniqueId = "Arcanum_Empty_ReligiousSchool" };

   public override string ToString() => UniqueId;

   #endregion
}