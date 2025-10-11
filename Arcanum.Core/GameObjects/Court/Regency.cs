using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Common.UI;

namespace Arcanum.Core.GameObjects.Court;

[ObjectSaveAs]
public partial class Regency : IEu5Object<Regency>
{
   # region Nexus Properties

   [SaveAs]
   [ParseAs("internally_assigned")]
   [DefaultValue(false)]
   [Description("The name of the regency.")]
   public bool IsInternallyAssigned { get; set; }

   [SaveAs]
   [ParseAs("modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [DefaultValue(null)]
   [Description("List of modifiers applied by this regency.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   # endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
#pragma warning restore AGS004

   public string GetNamespace => $"Character.{nameof(Regency)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.RegencySettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RegencyAgsSettings;
   public static Dictionary<string, Regency> GetGlobalItems() => Globals.Regencies;

   public static Regency Empty { get; } = new() { UniqueId = "Arcanum_Empty_Regency" };
}