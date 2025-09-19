using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.Attributes;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Common.UI;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs]
public partial class Climate : IEu5Object<Climate>
{
   #region Enums

   public enum WinterType
   {
      [EnumAgsData("none", true)]
      None,

      [EnumAgsData("mild", true)]
      Normal,

      [EnumAgsData("normal", true)]
      Mild,

      [EnumAgsData("severe", true)]
      Severe,
   }

   #endregion

#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueId { get; set; } = null!;
   public FileObj Source { get; set; } = null!;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(WinterType.None)]
   [ParseAs("winter")]
   [Description("What type of winter this climate has. Options are None, Light, Normal, and Harsh.")]
   public WinterType Winter { get; set; } = WinterType.None;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("color")]
   [Description("The Color the climate has on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("debug_color")]
   [Description("The debug color the climate has on the climate_screenshot mapmode.")]
   public JominiColor DebugMapColor { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(true)]
   [ParseAs("has_precipitation")]
   [Description("Whether this climate has precipitation.")]
   public bool HasPrecipitation { get; set; } = true;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("always_winter")]
   [Description("Whether this climate is always in winter, regardless of the season.")]
   public bool AlwaysWinter { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("unit_modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The unit modifier applied to units in provinces with this climate.")]
   public ObservableRangeCollection<ModValInstance> UnitModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("location_modifier", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The location modifier applied to provinces with this climate.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("audio_tags", AstNodeType.BlockNode, itemNodeType: AstNodeType.ContentNode)]
   [Description("The audio tags associated with this climate.")]
   public ObservableRangeCollection<AudioTag> AudioTags { get; set; } = [];

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.ClimateSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Climate Empty { get; } = new() { UniqueId = "Arcanum_Empty_Climate" };
   public static IEnumerable<Climate> GetGlobalItems() => Globals.Climates.Values;

   #endregion

   #region ISearchable

   public string GetNamespace => $"Map.{nameof(Climate)}";
   public string ResultName => UniqueId;
   public List<string> SearchTerms => [UniqueId];

   public void OnSearchSelected()
   {
      UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   }

   public ISearchResult VisualRepresentation => new SearchResultItem(null, UniqueId, string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;

   #endregion

   public AgsSettings AgsSettings => Config.Settings.AgsSettings.ClimateAgsSettings;
   public string SavingKey => UniqueId;

   public override string ToString() => UniqueId;
}