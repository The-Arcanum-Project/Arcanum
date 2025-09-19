using System.ComponentModel;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Jomini.ModifierSystem;
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

namespace Arcanum.Core.GameObjects.Map;

[ObjectSaveAs]
public partial class Vegetation : IEu5Object<Vegetation>
{
#pragma warning disable AGS004
   [ReadonlyNexus]
   [Description("Unique key of this object. Must be unique among all objects of this type.")]
   [DefaultValue("null")]
   public string UniqueKey { get; set; } = null!;
#pragma warning restore AGS004

   # region Nexus Properties

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("color")]
   [Description("The color associated with this vegetation type used on the map.")]
   public JominiColor Color { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("debug_color")]
   [Description("The debug color associated with this vegetation type used in the vegetation_screenshot mapmode.")]
   public JominiColor DebugColor { get; set; } = JominiColor.Empty;

   [SaveAs]
   [DefaultValue(1f)]
   [ParseAs("movement_cost")]
   [Description("The movement cost modifier for units moving through this vegetation type.")]
   public float MovementCost { get; set; } = 1f;

   [SaveAs]
   [DefaultValue(false)]
   [ParseAs("has_sand")]
   [Description("Whether this vegetation type includes sandy terrain, affecting certain gameplay mechanics.")]
   public bool HasSand { get; set; }

   [SaveAs]
   [DefaultValue(0)]
   [ParseAs("defender")]
   [Description("The defender bonus provided by this vegetation type in combat scenarios.")]
   public int DefenderDice { get; set; }

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("location_modifier", AstNodeType.BlockNode)]
   [Description("The location modifier applied to provinces with this climate.")]
   public ObservableRangeCollection<ModValInstance> LocationModifiers { get; set; } = [];

   [SaveAs]
   [DefaultValue(null)]
   [ParseAs("audio_tags", AstNodeType.BlockNode)]
   [Description("The audio tags associated with this climate.")]
   public ObservableRangeCollection<AudioTag> AudioTags { get; set; } = [];

   # endregion

   #region Interface Properties

   public bool IsReadonly => false;
   public NUISetting NUISettings => Config.Settings.NUIObjectSettings.VegetationSettings;
   public INUINavigation[] Navigations { get; } = [];
   public static Vegetation Empty { get; } = new() { UniqueKey = "Arcanum_Empty_Vegetation" };
   public static IEnumerable<Vegetation> GetGlobalItems() => Globals.Vegetation.Values;

   #endregion

   #region ISearchable

   public string GetNamespace => $"Map.{nameof(Vegetation)}";
   public string ResultName => UniqueKey;
   public List<string> SearchTerms => [UniqueKey];

   public void OnSearchSelected()
   {
      UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(this);
   }

   public ISearchResult VisualRepresentation { get; } = new SearchResultItem(null, "Vegetation", string.Empty);
   public IQueastorSearchSettings.Category SearchCategory
      => IQueastorSearchSettings.Category.MapObjects | IQueastorSearchSettings.Category.GameObjects;

   #endregion

   public AgsSettings AgsSettings { get; } = Config.Settings.AgsSettings.VegetationAgsSettings;
   public string SavingKey => UniqueKey;

   [SuppressAgs]
   public FileObj Source { get; set; } = null!;
}