using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

namespace Arcanum.Core.GameObjects.InGame.Cultural.SubObjects;

[ObjectSaveAs(savingMethod: "SaveCultureOpinionValue")]
public partial class CultureOpinionValue : IEu5Object<CultureOpinionValue>
{
   [SuppressAgs]
   [DefaultValue(null)]
   [Description("The culture this opinion is about.")]
   public Culture Key { get; set; } = Culture.Empty;

   [SuppressAgs]
   [DefaultValue(Opinion.Neutral)]
   [Description("The opinion value.")]
   public Opinion Value { get; set; } = Opinion.Neutral;

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.OpinionValueSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CultureOpinionValue;
   public string UniqueId { get; set; } = string.Empty;
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public string SavingKey => string.Empty;
   public InjRepType InjRepType { get; set; }
   public static CultureOpinionValue Empty { get; } = new() { Key = Culture.Empty, Value = Opinion.Neutral };

   #endregion

   #region Equality Members

   protected bool Equals(CultureOpinionValue other) => Key.Equals(other.Key) && Value.Equals(other.Value);

   public static Dictionary<string, CultureOpinionValue> GetGlobalItems() => [];

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((CultureOpinionValue)obj);
   }

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(Key, Value);

   #endregion

   public override string ToString() => $"{Key.UniqueId}: {Value}";
   public string GetNamespace => $"Cultural.{nameof(CultureOpinionValue)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, ToString(), string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
}