using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Religion.SubObjects;

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
   [ReadonlyNexus]
   [Description("Unique key of this ReligiousSchoolRelations. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Religion.{nameof(ReligiousSchoolRelations)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ReligiousSchoolRelationsSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ReligiousSchoolRelationsAgsSettings;

   public static Dictionary<string, ReligiousSchoolRelations> GetGlobalItems()
      => Globals.State.ReligiousSchoolRelations;

   public static ReligiousSchoolRelations Empty { get; } =
      new() { UniqueId = "Arcanum_Empty_ReligiousSchoolRelations" };

   public override string ToString() => UniqueId;

   #endregion
}