using System.ComponentModel;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.Settings.BaseClasses;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MapSettingsObj() : InternalSearchableSetting(Config.Settings)
{
   [Description("If animations are used on map borders.")]
   [DefaultValue(true)]
   public bool AllowAnimatedBorders
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If animations are used on the map.")]
   [DefaultValue(true)]
   public bool AllowAnimations
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If the map can be animated.")]
   [DefaultValue(true)]
   public bool AllowAnimatedMap
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("The maximum FPS for map animations. Higher values may cause performance issues.")]
   [DefaultValue(30)]
   public int MaxAnimationFps
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 30;

   [Description("The maximum FPS for the map rendering. Higher values may cause performance issues.")]
   [DefaultValue(60)]
   public int MaxMapFps
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 60;

   [Description("The maximum zoom level for the map.")]
   [DefaultValue(8f)]
   public float MaxZoomLevel
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 8f;

   [Description("The minimum zoom level for the map.")]
   [DefaultValue(-1.1f)]
   public float MinZoomLevel
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = -1.1f;

   [Description("If map updates should be suspended when possible to improve performance.")]
   [DefaultValue(true)]
   public bool SuspendMapUpdatesWhenPossible
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("The factor by which the opacity of map elements is reduced when hovering over them.")]
   [DefaultValue(0.8f)]
   public float PrviewOpacityFactor
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 0.8f;

   [Description("The color of the border when hovering over a map element.")]
   [DefaultValue(typeof(Color), "Transparent")]
   public Color HoverBorderColor
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = Colors.Transparent;

   [Description("The color of the border when highlighting a map element.")]
   [DefaultValue(typeof(Color), "Yellow")]
   public Color HighlightBorderColor
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = Colors.Yellow;

   [Description("The color of the border when selecting a map element.")]
   [DefaultValue(typeof(Color), "Red")]
   public Color SelectedBorderColor
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = Colors.Red;

   [Description("The type of border to use for a selected location.")]
   [DefaultValue(BorderType.Simple)]
   public BorderType SelectedBorderType
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = BorderType.Simple;

   [Description("The type of border to use for a hovered location.")]
   [DefaultValue(BorderType.Wide)]
   public BorderType HoverBorderType
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = BorderType.Wide;

   [Description("The type of border to use for a highlighted location.")]
   [DefaultValue(BorderType.Dotted)]
   public BorderType HighlightBorderType
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = BorderType.Dotted;

   [Description("If the border of a selected location should be animated.")]
   [DefaultValue(true)]
   public bool AnimatedSelectionBorder
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If the border of a hovered location should be animated.")]
   [DefaultValue(false)]
   public bool AnimatedHoverBorder
   {
      get;
      set => SetNotifyProperty(ref field, value);
   }

   [Description("If the border of a highlighted location should be animated.")]
   [DefaultValue(true)]
   public bool AnimatedHighlightBorder
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If water locations should use a shade of the WaterShadeBaseColor as color or the color from the location definition.")]
   [DefaultValue(true)]
   public bool UseShadeOfColorOnWater
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("The base color used to shade water locations when 'Use Shade Of Color On Water' is enabled.")]
   [DefaultValue(typeof(Color), "0, 105, 148")]
   public Color WaterShadeBaseColor
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = Color.FromRgb(0, 105, 148);

   [Description("Wheter any tooltip is shown when hovering over map locations.")]
   [DefaultValue(true)]
   public bool ShowTooltips
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("The opacity of the selection color applied to selected map elements.")]
   [DefaultValue(0.5f)]
   public float SelectionColorOpacity
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 0.5f;

   [Description("The opacity of the highlight color applied to highlighted map elements.")]
   [DefaultValue(0.5f)]
   public float HighlightColorOpacity
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 0.5f;

   [Description("The opacity of the frozen selection color applied to frozen selected map elements.")]
   [DefaultValue(0.5f)]
   public float FrozenSelectionColorOpacity
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 0.5f;
}