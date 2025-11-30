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
using Arcanum.Core.GameObjects.Economy.SubClasses;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ObjectSaveAs]
[NexusConfig]
public partial class TownSetup : IEu5Object<TownSetup>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("-", isShatteredList: true, iEu5KeyType: typeof(Building), itemNodeType: AstNodeType.ContentNode)]
   [Description("Collection of building levels associated with this TownSetup.")]
   public ObservableRangeCollection<BuildingLevel> BuildingLevels { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this TownSetup. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Namespace.{nameof(TownSetup)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.TownSetupSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.TownSetupAgsSettings;
   public static Dictionary<string, TownSetup> GetGlobalItems() => Globals.TownSetups;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static TownSetup Empty { get; } = new() { UniqueId = "Arcanum_Empty_TownSetup" };

   #endregion
}