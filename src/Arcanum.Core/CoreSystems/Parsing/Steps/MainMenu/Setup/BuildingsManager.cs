using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.Economy;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ObjectSaveAs]
[NexusConfig]
public partial class BuildingsManager : IEu5Object<BuildingsManager>
{
   #region Nexus Properties

   [SaveAs(isEmbeddedObject: true)]
   [ParseAs("-", itemNodeType: AstNodeType.BlockNode, iEu5KeyType: typeof(Building))]
   [DefaultValue(null)]
   [Description("List of building definitions managed by this BuildingsManager.")]
   public ObservableRangeCollection<BuildingDefinition> BuildingDefinitions { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this BuildingsManager. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Setup.{nameof(BuildingsManager)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.BuildingsManagerSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.BuildingsManagerAgsSettings;

   public static Dictionary<string, BuildingsManager> GetGlobalItems() => new()
   {
      { "BuildingsManager", Globals.BuildingsManager },
   };

   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static BuildingsManager Empty { get; } = new() { UniqueId = "Arcanum_Empty_BuildingsManager" };

   #endregion
}