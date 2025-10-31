using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map.SubObjects;
using Arcanum.Core.GameObjects.Religious;
using Common.Logger;
using Common.UI;

namespace Arcanum.Core.GameObjects.Map;

[ParserFor(typeof(MapMovementAssist))]
public static partial class MapMovementAssistParsingWhy;

[ObjectSaveAs]
public partial class LocationTemplateData : IEu5Object<LocationTemplateData>, IMapInferable
{
   #region Nexus Properties

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("topography")]
   [Description("The topography type of this location template.")]
   [DefaultValue(null)]
   public Topography Topography { get; set; } = Topography.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("vegetation")]
   [Description("The vegetation type of this location template.")]
   [DefaultValue(null)]
   public Vegetation Vegetation { get; set; } = Vegetation.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("climate")]
   [Description("The climate type of this location template.")]
   [DefaultValue(null)]
   public Climate Climate { get; set; } = Climate.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("religion")]
   [Description("The dominant religion of this location template.")]
   [DefaultValue(null)]
   public Religious.Religion Religion { get; set; } = Religious.Religion.Empty;

   [SaveAs(SavingValueType.Identifier)]
   [ParseAs("culture")]
   [Description("The dominant culture of this location template.")]
   [DefaultValue(null)]
   public Cultural.Culture Culture { get; set; } = Cultural.Culture.Empty;

   [SaveAs]
   [ParseAs("raw_material")]
   [Description("The raw material abundance of this location template.")]
   [DefaultValue(null)]
   public RawMaterial RawMaterial { get; set; } = RawMaterial.Empty;

   [SaveAs]
   [ParseAs("natural_harbor_suitability")]
   [Description("The natural harbor suitability of this location template.")]
   [DefaultValue(0f)]
   public float NaturalHarborSuitability { get; set; }

   [SaveAs]
   [ParseAs("movement_assistance", customParser: "MapMovementAssistParsing", nodeType: AstNodeType.BlockNode)]
   [Description("The movement assistance provided by this location template.")]
   [DefaultValue(null)]
   public MapMovementAssist MovementAssistance { get; set; } = MapMovementAssist.Empty;

   [SaveAs]
   [ParseAs("modifier")]
   [Description("Static modifiers applied to this location template.")]
   [DefaultValue(null)]
   public StaticModifier StaticModifier { get; set; } = StaticModifier.Empty;

   #endregion

#pragma warning disable AGS004
   [Description("Unique key of this LocationTemplateData. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;

   [SuppressAgs]
   public Eu5FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   #region IEu5Object

   public string GetNamespace => $"Map.{nameof(LocationTemplateData)}";
   public void OnSearchSelected() => UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, GetNamespace.Replace('.', '>'));
   public Enum SearchCategory => IQueastorSearchSettings.DefaultCategories.GameObjects;
   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.LocationTemplateDataSettings;
   public INUINavigation[] Navigations => [];
   public AgsSettings AgsSettings => Config.Settings.AgsSettings.LocationTemplateDataAgsSettings;
   public static Dictionary<string, LocationTemplateData> GetGlobalItems() => Globals.LocationTemplateData;
   public Eu5ObjectLocation FileLocation { get; set; } = Eu5ObjectLocation.Empty;

   public static LocationTemplateData Empty { get; } = new() { UniqueId = "Arcanum_Empty_LocationTemplateData" };

   public override string ToString() => UniqueId;

   #endregion

   public MapModeManager.MapModeType GetMapMode => MapModeManager.MapModeType.Base;

   public List<IEu5Object> GetInferredList(IEnumerable<Location> sLocs)
      => sLocs.Where(loc => loc.TemplateData == this).Cast<IEu5Object>().ToList();

   public List<Location> GetRelevantLocations(IEu5Object[] items)
   {
      Debug.Assert(items.All(x => x is LocationTemplateData));
      var objs = items.Cast<LocationTemplateData>().ToArray();

      List<Location> locations = [];

      foreach (var loc in Globals.Locations.Values)
         if (objs.Contains(loc.TemplateData) &&
             loc.TemplateData != Empty)
            locations.Add(loc);

      return locations;
   }
}