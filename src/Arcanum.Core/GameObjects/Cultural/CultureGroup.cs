using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;

namespace Arcanum.Core.GameObjects.Cultural;

[ObjectSaveAs]
public partial class CultureGroup : IEu5Object<CultureGroup>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("country_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to countries of this culture group.")]
   public ObservableRangeCollection<ModValInstance> CountryModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("character_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to characters of this culture group.")]
   public ObservableRangeCollection<ModValInstance> CharacterModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("location_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to locations of this culture group.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this CultureGroup. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Cultural.{nameof(CultureGroup)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CultureGroupSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CultureGroupAgsSettings;
   public static Dictionary<string, CultureGroup> GetGlobalItems() => Globals.CultureGroups;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static CultureGroup Empty { get; } = new() { UniqueId = "Arcanum_Empty_CultureGroup" };

   public override string ToString() => UniqueId;

   #endregion
}