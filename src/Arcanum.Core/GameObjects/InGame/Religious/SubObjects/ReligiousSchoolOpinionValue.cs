using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Cultural;

namespace Arcanum.Core.GameObjects.InGame.Religious.SubObjects;

[ObjectSaveAs(savingMethod: "SaveReligiousSchoolOpinionValue")]
#pragma warning disable ARC002
public partial class ReligiousSchoolOpinionValue
   : IEu5Object<ReligiousSchoolOpinionValue>
#pragma warning restore ARC002
{
   [SuppressAgs]
   [DefaultValue(null)]
   [Description("The culture this opinion is about.")]
   public ReligiousSchool Key { get; set; } = ReligiousSchool.Empty;

   [SuppressAgs]
   [DefaultValue(Opinion.Neutral)]
   [Description("The opinion value.")]
   public Opinion Value { get; set; } = Opinion.Neutral;

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousSchoolOpinionValueSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousSchoolOpinionValue;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string SavingKey => string.Empty;
   public InjRepType InjRepType { get; set; }
   public static ReligiousSchoolOpinionValue Empty { get; } =
      new() { Key = ReligiousSchool.Empty, Value = Opinion.Neutral };

   #endregion

   #region Equality Members

   protected bool Equals(ReligiousSchoolOpinionValue other) => Key.Equals(other.Key) && Value.Equals(other.Value);

   #endregion

   public string GetNamespace => "Religion.ReligiousSchoolOpinionValue";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public static Dictionary<string, ReligiousSchoolOpinionValue> GetGlobalItems() => [];
}