#region

using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;
using Arcanum.Core.GameObjects.InGame.Court.State;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Economy.SubClasses;
using Arcanum.Core.GameObjects.InGame.Map;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Nexus.Core.Attributes;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Economy;

[NexusConfig]
[ObjectSaveAs]
public partial class RawMaterial : IEu5Object<RawMaterial>, IMapInferable
{
   #region Nexus Properties

   [SaveAs]
   [ParseAs("development_threshold")]
   [Description("The development threshold required for this raw material to appear.")]
   [DefaultValue(0)]
   public int DevelopmentThreshold { get; set; }

   [SaveAs]
   [ParseAs("method")]
   [Description("The method by which this raw material is obtained.")]
   [DefaultValue(MaterialGatheringMethod.Farming)]
   public MaterialGatheringMethod Method { get; set; } = MaterialGatheringMethod.Farming;

   [SaveAs]
   [ParseAs("category")]
   [Description("The category of this raw material.")]
   [DefaultValue(MaterialCategory.RawMaterial)]
   public MaterialCategory Category { get; set; } = MaterialCategory.RawMaterial;

   [SaveAs]
   [ParseAs("color")]
   [Description("The color associated with this raw material.")]
   [DefaultValue(null)]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [ParseAs("is_slaves")]
   [Description("Indicates whether this raw material is a slave good.")]
   [DefaultValue(false)]
   public bool IsSlaves { get; set; }

   [SaveAs]
   [ParseAs("block_rgo_upgrade")]
   [Description("Indicates whether this raw material blocks RGO upgrades.")]
   [DefaultValue(false)]
   public bool BlockRgoUpgrade { get; set; }

   [SaveAs]
   [ParseAs("inflation")]
   [Description("Indicates wheterh this raw material increaes inflation.")]
   [DefaultValue(false)]
   public bool Inflation { get; set; }

   [SaveAs]
   [ParseAs("origin_in_old_world")]
   [Description("Indicates whether this raw material originates in the Old World.")]
   [DefaultValue(false)]
   public bool OriginInOldWorld { get; set; }

   [SaveAs]
   [ParseAs("origin_in_new_world")]
   [Description("Indicates whether this raw material originates in the New World.")]
   [DefaultValue(false)]
   public bool OriginInNewWorld { get; set; }

   [SaveAs]
   [ParseAs("no_demand_if_no_market_availability")]
   [Description("Indicates whether this raw material has no demand if it's not available in the market.")]
   [DefaultValue(false)]
   public bool NoDemandIfNoMarketAvailability { get; set; }

   [SaveAs]
   [ParseAs("base_production")]
   [Description("The base production amount of this raw material.")]
   [DefaultValue(0f)]
   public float BaseProduction { get; set; }

   [SaveAs]
   [ParseAs("market_unavailability_demand_modifier")]
   [Description("The demand modifier applied when this raw material is unavailable in the market.")]
   [DefaultValue(0f)]
   public float MarketUnavailabilityDemandModifier { get; set; }

   [SaveAs]
   [ParseAs("food")]
   [Description("How much food this raw material provides.")]
   [DefaultValue(0f)]
   public float Food { get; set; }

   [SaveAs]
   [ParseAs("transport_cost")]
   [Description("The transport cost associated with this raw material.")]
   [DefaultValue(1f)]
   public float TransportCost { get; set; } = 1f;

   [SaveAs]
   [ParseAs("default_market_price")]
   [Description("The default market price of this raw material.")]
   [DefaultValue(1f)]
   public float DefaultMarketPrice { get; set; } = 1f;

   [SaveAs]
   [ParseAs("disease_demand_modifier")]
   [Description("The disease demand modifier for this raw material.")]
   [DefaultValue(1f)]
   public float DiseaseDemandModifier { get; set; } = 1f;

   [SaveAs]
   [ParseAs("ai_rgo_size_importance")]
   [Description("ai preference to avoid building a city on this rgo.")]
   [DefaultValue(1f)]
   public float AiRgoSizeImportImportance { get; set; } = 1f;

   [SaveAs]
   [ParseAs("ai_rgo_expansion_priority")]
   [Description("The importance of this raw material for AI when expanding rgos.")]
   [DefaultValue(1f)]
   public float AiRgoExpansionPriority { get; set; } = 1f;

   [SaveAs]
   [ParseAs("max_rgo_size")]
   [Description("The maximum size of RGOs that can be built with this raw material.")]
   [DefaultValue(1f)]
   public float MaxRgoSize { get; set; } = 1f;

   [SaveAs]
   [ParseAs("literacy_demand_factor")]
   [Description("The factor by which literacy affects the demand for this raw material.")]
   [DefaultValue(0f)]
   public float LiteracyDemandFactor { get; set; } = 0f;

   [SaveAs]
   [ParseAs("demand_only_in_sub_continent")]
   [Description("Indicates whether this raw material can only be demanded in specific sub-continents.")]
   [DefaultValue(null)]
   public SubContinent DemandOnlyInSubContinent { get; set; } = SubContinent.Empty;

   [SaveAs]
   [ParseAs("demand_only_in_continent")]
   [Description("Indicates whether this raw material can only be demanded in specific continents.")]
   [DefaultValue(null)]
   public Continent DemandOnlyInContinent { get; set; } = Continent.Empty;

   [SaveAs]
   [ParseAs("demand_only_in_region")]
   [Description("Indicates whether this raw material can only be demanded in specific regions.")]
   [DefaultValue(null)]
   public Region DemandOnlyInRegion { get; set; } = Region.Empty;

   [SaveAs]
   [ParseAs("custom_tags", itemNodeType: AstNodeType.KeyOnlyNode)]
   [Description("A list of custom strings to use to identify that good.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<string> CustomTags { get; set; } = [];

   [SaveAs]
   [ParseAs("demand_add", itemNodeType: AstNodeType.ContentNode)]
   [Description("The demand addition for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<DemandData> DemandAdd { get; set; } = [];

   [SaveAs]
   [ParseAs("wealth_impact_threshold", itemNodeType: AstNodeType.ContentNode)]
   [Description("The wealth impact threshold for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<WealthImpactData> WealthImpactData { get; set; } = [];

   [SaveAs]
   [ParseAs("demand_multiply", itemNodeType: AstNodeType.ContentNode)]
   [Description("The demand multiplikation for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<DemandData> DemandMultiply { get; set; } = [];

   [SaveAs]
   [ParseAs("winter_demand_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description(" The winter demand modifier for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<WinterGoodsDemand> WinterGoodsDemands { get; set; } = [];

   [SaveAs]
   [ParseAs("continent_demand_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Indicates whether this raw material can only be demanded in specific continents.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<ContinentDemand> ContinentDemandModifier { get; set; } = [];

   [SaveAs]
   [ParseAs("climate_demand_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description("Indicates whether this raw material can only be demanded in specific climates.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<ClimateDemand> ClimateDemandModifier { get; set; } = [];

   [SaveAs]
   [ParseAs("topography_demand_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description(" The topography demand modifier for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<TopographyGoodsDemand> TopographyGoodsDemands { get; set; } = [];

   [SaveAs]
   [ParseAs("sub_continent_demand_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description(" The sub-continent demand modifier for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<SubContinentGoodsDemand> SubContinentGoodsDemands { get; set; } = [];

   [SaveAs]
   [ParseAs("region_demand_modifier", itemNodeType: AstNodeType.ContentNode)]
   [Description(" The region demand modifier for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<RegionGoodsDemand> RegionGoodsDemands { get; set; } = [];

   [SaveAs(SavingValueType.IAgs, isEmbeddedObject: true, saveEmbeddedAsIdentifier: false)]
   [ParseAs("wealth_impact_threshold", itemNodeType: AstNodeType.BlockNode, iEu5KeyType: typeof(Estate))]
   [Description("The wealth impact thresholds for this raw material.")]
   [DefaultValue(null)]
   public ObservableRangeCollection<EstateSatisfactionDefinition> EstateAttributes { get; set; } = [];

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this RawMaterial. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = Eu5FileObj.Empty;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Economy.{nameof(RawMaterial)}";
   public void OnSearchSelected() => SelectionManager.Eu5ObjectSelectedInSearch(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => true;
   public INUINavigation[] Navigations => [];
   public InjRepType InjRepType { get; set; } = InjRepType.None;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.RawMaterialSettings;
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.RawMaterial;
   public static Dictionary<string, RawMaterial> GetGlobalItems() => Globals.RawMaterials;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static RawMaterial Empty { get; } = new() { UniqueId = "Arcanum_Empty_RawMaterial" };

   #endregion

   #region IMapInferable

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Goods;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
   {
      HashSet<IEu5Object> items = [];
      foreach (var loc in sLocs)
      {
         if (loc.TemplateData == LocationTemplateData.Empty && loc.TemplateData.RawMaterial != Empty)
            continue;

         items.Add(loc.TemplateData.RawMaterial);
      }

      return items.ToList();
   }

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is RawMaterial));
      var objs = items.Cast<RawMaterial>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (objs.Contains(loc.TemplateData.RawMaterial) &&
             loc.TemplateData != LocationTemplateData.Empty &&
             loc.TemplateData.RawMaterial != Empty)
            locations.Add(loc);

      return locations;
   }

   #endregion
}