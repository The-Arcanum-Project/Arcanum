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

namespace Arcanum.Core.GameObjects.Cultural.SubObjects;

[ObjectSaveAs(savingMethod: "SaveIAgsEnumKvp")]
public partial class ReligionOpinionValue : IEu5Object<ReligionOpinionValue>
{
   #region Nexus Properties

   [SuppressAgs]
   [DefaultValue(null)]
   [Description("The culture this opinion is about.")]
   public Religious.Religion Key { get; set; } = Religious.Religion.Empty;

   [SuppressAgs]
   [DefaultValue(Opinion.Neutral)]
   [Description("The opinion value.")]
   public Opinion Value { get; set; } = Opinion.Neutral;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this Eu5ObjOpinionValue. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Opinions.{nameof(ReligionOpinionValue)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.Eu5ObjOpinionValueSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.Eu5ObjOpinionValueAgsSettings;
   public static Dictionary<string, ReligionOpinionValue> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static ReligionOpinionValue Empty { get; } = new() { UniqueId = "Arcanum_Empty_Eu5ObjOpinionValue" };

   public override string ToString() => UniqueId;

   #endregion
}