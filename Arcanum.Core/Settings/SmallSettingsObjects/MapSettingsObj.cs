using System.ComponentModel;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Selection;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MapSettingsObj
{
   [Description("If animations are used on map borders.")]
   [DefaultValue(true)]
   public bool AllowAnimatedBorders { get; set; } = true;

   [Description("If animations are used on the map.")]
   [DefaultValue(true)]
   public bool AllowAnimations { get; set; } = true;

   [Description("If the map can be animated.")]
   [DefaultValue(true)]
   public bool AllowAnimatedMap { get; set; } = true;

   [Description("The maximum FPS for map animations. Higher values may cause performance issues.")]
   [DefaultValue(30)]
   public int MaxAnimationFps { get; set; } = 30;

   [Description("The maximum FPS for the map rendering. Higher values may cause performance issues.")]
   [DefaultValue(60)]
   public int MaxMapFps { get; set; } = 60;

   [Description("The maximum zoom level for the map.")]
   [DefaultValue(8f)]
   public float MaxZoomLevel { get; set; } = 8f;

   [Description("The minimum zoom level for the map.")]
   [DefaultValue(-1.1f)]
   public float MinZoomLevel { get; set; } = -1.1f;

   [Description("If map updates should be suspended when possible to improve performance.")]
   [DefaultValue(true)]
   public bool SuspendMapUpdatesWhenPossible { get; set; } = true;

   [Description("The factor by which the opacity of map elements is reduced when hovering over them.")]
   [DefaultValue(0.8f)]
   public float HoverOpacityFactor { get; set; } = 0.8f;

   [Description("The color of the border when hovering over a map element.")]
   [DefaultValue(typeof(Color), "Transparent")]
   public Color HoverBorderColor { get; set; } = Colors.Transparent;

   [Description("The color of the border when highlighting a map element.")]
   [DefaultValue(typeof(Color), "Yellow")]
   public Color HighlightBorderColor { get; set; } = Colors.Yellow;

   [Description("The color of the border when selecting a map element.")]
   [DefaultValue(typeof(Color), "Red")]
   public Color SelectedBorderColor { get; set; } = Colors.Red;

   [Description("The type of border to use for a selected location.")]
   [DefaultValue(BorderType.Simple)]
   public BorderType SelectedBorderType { get; set; } = BorderType.Simple;

   [Description("The type of border to use for a hovered location.")]
   [DefaultValue(BorderType.Wide)]
   public BorderType HoverBorderType { get; set; } = BorderType.Wide;

   [Description("The type of border to use for a highlighted location.")]
   [DefaultValue(BorderType.Dotted)]
   public BorderType HighlightBorderType { get; set; } = BorderType.Dotted;

   [Description("If the border of a selected location should be animated.")]
   [DefaultValue(true)]
   public bool AnimatedSelectionBorder { get; set; } = true;

   [Description("If the border of a hovered location should be animated.")]
   [DefaultValue(false)]
   public bool AnimatedHoverBorder { get; set; } = false;

   [Description("If the border of a highlighted location should be animated.")]
   [DefaultValue(true)]
   public bool AnimatedHighlightBorder { get; set; } = true;
}