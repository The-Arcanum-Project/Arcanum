using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Common.UI;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Economy.SubClasses;

[ObjectSaveAs]
[NexusConfig]
public partial class BuildingLevel : IEu5Object<BuildingLevel>
{
   #region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("-", iEu5KeyType: typeof(Building))]
   [Description("The estate this definition applies to.")]
   public Building Building { get; set; } = Building.Empty;

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("-")]
   [Description("The building level of this building.")]
   public int Level { get; set; }

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this BuildingLevel. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Economy.Building.{nameof(BuildingLevel)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.BuildingLevelSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.BuildingLevelAgsSettings;
   public static Dictionary<string, BuildingLevel> GetGlobalItems() => Globals.BuildingLevels;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static BuildingLevel Empty { get; } = new() { UniqueId = "Arcanum_Empty_BuildingLevel" };

   #endregion
}