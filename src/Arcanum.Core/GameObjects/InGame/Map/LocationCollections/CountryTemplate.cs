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
using Nexus.Core;
using Nexus.Core.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

[ObjectSaveAs]
[NexusConfig]
public partial class CountryTemplate : IEu5Object<CountryTemplate>
{
   #region Nexus Properties

   [Description("The country that this template is based on.")]
   [DefaultValue(null)]
   [ParseAs(Globals.DO_NOT_PARSE_ME)]
   [SaveAs]
   public Country TemplateData { get; set; } = Country.Empty;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this CountryTemplate. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.LocationCollections.{nameof(CountryTemplate)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CountryTemplateSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CountryTemplate;
   public static Dictionary<string, CountryTemplate> GetGlobalItems() => Globals.CountryTemplates;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   [IgnoreModifiable]
   [SuppressAgs]
   public int Index { get; init; }

   public static CountryTemplate Empty { get; } = new()
   {
      UniqueId = "Arcanum_Empty_CountryTemplate", Index = -1,
   };

   #endregion
}