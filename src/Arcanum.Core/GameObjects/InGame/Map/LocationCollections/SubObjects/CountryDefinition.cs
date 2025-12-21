using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Religious;
using Common.UI;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;

[ObjectSaveAs]
public partial class CountryDefinition : IEu5Object<CountryDefinition>
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("is_historic")]
   [DefaultValue(false)]
   [Description("Whether this country is historic")]
   public bool IsHistoric { get; set; }

   [SaveAs]
   [ParseAs("color")]
   [DefaultValue(null)]
   [Description("The color key of this country")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("color2")]
   [DefaultValue(null)]
   [Description("The color2 key of this country")]
   public JominiColor Color2 { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("unit_color0")]
   [DefaultValue(null)]
   [Description("The unit_color0 key of this country")]
   public JominiColor UnitColor0 { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("unit_color1")]
   [DefaultValue(null)]
   [Description("The unit_color1 key of this country")]
   public JominiColor UnitColor1 { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("unit_color2")]
   [DefaultValue(null)]
   [Description("The unit_color2 key of this country")]
   public JominiColor UnitColor2 { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("culture_definition")]
   [DefaultValue(null)]
   [Description("The culture definition of this country")]
   public Culture CultureDefinition { get; set; } = Culture.Empty;

   [SaveAs]
   [ParseAs("religion_definition")]
   [DefaultValue(null)]
   [Description("The religion definition of this country")]
   public Religion ReligionDefinition { get; set; } = Religion.Empty;

   [SaveAs]
   [ParseAs("description_category")]
   [DefaultValue(DescriptionCategory.Administrative)]
   [Description("The description category of this country")]
   public DescriptionCategory DescriptionCategory { get; set; } = DescriptionCategory.Administrative;

   [SaveAs]
   [ParseAs("difficulty")]
   [DefaultValue(0)]
   [Description("The difficulty level of this country, between 0 and 5")]
   public int Difficulty { get; set; }

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this CountryDefinition. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.Country.{nameof(CountryDefinition)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.CountryDefinitionSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.CountryDefinition;
   public static Dictionary<string, CountryDefinition> GetGlobalItems() => Globals.CountryDefinitions;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;
   public InjRepType InjRepType { get; set; } = InjRepType.None;

   public static CountryDefinition Empty { get; } = new() { UniqueId = "Arcanum_Empty_CountryDefinition" };

   public override string ToString() => UniqueId;

   #endregion
}