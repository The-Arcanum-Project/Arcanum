using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
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
using Country = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Country;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;

namespace Arcanum.Core.GameObjects.MainMenu.States;

[ObjectSaveAs]
[NexusConfig]
public partial class BuildingDefinition : IEu5Object<BuildingDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [PropertyConfig(isRequired: true)]
   [ParseAs("tag")]
   [Description("The country that owns this building.")]
   [DefaultValue(null)]
   public Country Owner { get; set; } = Country.Empty;

   [SaveAs]
   [PropertyConfig(isRequired: true)]
   [ParseAs("level")]
   [Description("The level of this building.")]
   [DefaultValue(1)]
   public int Level { get; set; } = 1;

   [SaveAs]
   [PropertyConfig(isRequired: true)]
   [ParseAs("location")]
   [Description("The location where this building is situated.")]
   [DefaultValue(null)]
   public Location Location { get; set; } = Location.Empty;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this BuildingDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Economy.Building.{nameof(BuildingDefinition)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.BuildingDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.BuildingDefinition;
   public static Dictionary<string, BuildingDefinition> GetGlobalItems() => [];
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static BuildingDefinition Empty { get; } = new() { UniqueId = "Arcanum_Empty_BuildingDefinition" };

   #endregion
}