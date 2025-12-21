using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Religious.SubObjects;

[NexusConfig]
[ObjectSaveAs]
public partial class ReligiousSchoolRelations : IEu5Object<ReligiousSchoolRelations>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("relation", itemNodeType: AstNodeType.ContentNode)]
   [Description("The opinion towards other religious schools.")]
   public ObservableRangeCollection<ReligiousSchoolOpinionValue> Relations { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs(" ", isShatteredList: true, itemNodeType: AstNodeType.ContentNode)]
   // If any key is null we look for a Type's UniqueId that matches the value by providing the type in the ParseAs attribute
   [Description("Relations towards other schools")]
   public ObservableRangeCollection<ReligiousSchoolOpinionValue> ShatteredRelations { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this ReligiousSchoolRelations. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligiousSchoolRelations)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousSchoolRelationsSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousSchoolRelations;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static Dictionary<string, ReligiousSchoolRelations> GetGlobalItems() => Globals.State.ReligiousSchoolRelations;

   public static ReligiousSchoolRelations Empty { get; } =
      new() { UniqueId = "Arcanum_Empty_ReligiousSchoolRelations" };

   #endregion
}