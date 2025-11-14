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
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.Court;

[NexusConfig]
[ObjectSaveAs]
public partial class ParliamentType : IEu5Object<ParliamentType>
{
   #region Nexus Properties

   [ParseAs("type")]
   [DefaultValue(State.SubClasses.ParliamentType.Country)]
   [SaveAs]
   [Description("The type of this Parliament.")]
   public State.SubClasses.ParliamentType Type { get; set; } = State.SubClasses.ParliamentType.Country;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Modifiers applied to the country with this ParliamentType.")]
   public ObservableRangeCollection<ModValInstance> Modifiers { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this ParliamentType. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Court.{nameof(ParliamentType)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ParliamentTypeSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ParliamentTypeAgsSettings;
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public static Dictionary<string, ParliamentType> GetGlobalItems() => Globals.ParliamentTypes;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static ParliamentType Empty { get; } = new() { UniqueId = "Arcanum_Empty_ParliamentType" };

   #endregion
}