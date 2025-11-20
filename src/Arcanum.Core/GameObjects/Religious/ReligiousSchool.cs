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

namespace Arcanum.Core.GameObjects.Religious;

[NexusConfig]
[ObjectSaveAs]
public partial class ReligiousSchool : IEu5Object<ReligiousSchool>
{
   #region Nexus Properties

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this ReligiousSchool. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligiousSchool)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousSchoolSettings;
   public INUINavigation[] Navigations => [];
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousSchoolAgsSettings;
   public static Dictionary<string, ReligiousSchool> GetGlobalItems() => Globals.ReligiousSchools;

   public static ReligiousSchool Empty { get; } = new() { UniqueId = "Arcanum_Empty_ReligiousSchool" };

   #endregion
}