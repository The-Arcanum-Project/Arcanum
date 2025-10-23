using System.ComponentModel;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.Settings.BaseClasses;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MapSettingsObj() : InternalSearchableSetting(Config.Settings)
{
   private bool _allowAnimatedBorders = true;
   private bool _allowAnimations = true;
   private bool _allowAnimatedMap = true;
   private int _maxAnimationFps = 30;
   private int _maxMapFps = 60;
   private float _maxZoomLevel = 8f;
   private float _minZoomLevel = -1.1f;
   private bool _suspendMapUpdatesWhenPossible = true;
   private float _hoverOpacityFactor = 0.8f;
   private Color _hoverBorderColor = Colors.Transparent;
   private Color _highlightBorderColor = Colors.Yellow;
   private Color _selectedBorderColor = Colors.Red;
   private BorderType _selectedBorderType = BorderType.Simple;
   private BorderType _hoverBorderType = BorderType.Wide;
   private BorderType _highlightBorderType = BorderType.Dotted;
   private bool _animatedSelectionBorder = true;
   private bool _animatedHoverBorder = false;
   private bool _animatedHighlightBorder = true;
   [Description("If animations are used on map borders.")]
   [DefaultValue(true)]
   public bool AllowAnimatedBorders
   {
      get => _allowAnimatedBorders;
      set => SetNotifyProperty(ref _allowAnimatedBorders, value);
   }

   [Description("If animations are used on the map.")]
   [DefaultValue(true)]
   public bool AllowAnimations
   {
      get => _allowAnimations;
      set => SetNotifyProperty(ref _allowAnimations, value);
   }

   [Description("If the map can be animated.")]
   [DefaultValue(true)]
   public bool AllowAnimatedMap
   {
      get => _allowAnimatedMap;
      set => SetNotifyProperty(ref _allowAnimatedMap, value);
   }

   [Description("The maximum FPS for map animations. Higher values may cause performance issues.")]
   [DefaultValue(30)]
   public int MaxAnimationFps
   {
      get => _maxAnimationFps;
      set => SetNotifyProperty(ref _maxAnimationFps, value);
   }

   [Description("The maximum FPS for the map rendering. Higher values may cause performance issues.")]
   [DefaultValue(60)]
   public int MaxMapFps
   {
      get => _maxMapFps;
      set => SetNotifyProperty(ref _maxMapFps, value);
   }

   [Description("The maximum zoom level for the map.")]
   [DefaultValue(8f)]
   public float MaxZoomLevel
   {
      get => _maxZoomLevel;
      set => SetNotifyProperty(ref _maxZoomLevel, value);
   }

   [Description("The minimum zoom level for the map.")]
   [DefaultValue(-1.1f)]
   public float MinZoomLevel
   {
      get => _minZoomLevel;
      set => SetNotifyProperty(ref _minZoomLevel, value);
   }

   [Description("If map updates should be suspended when possible to improve performance.")]
   [DefaultValue(true)]
   public bool SuspendMapUpdatesWhenPossible
   {
      get => _suspendMapUpdatesWhenPossible;
      set => SetNotifyProperty(ref _suspendMapUpdatesWhenPossible, value);
   }

   [Description("The factor by which the opacity of map elements is reduced when hovering over them.")]
   [DefaultValue(0.8f)]
   public float HoverOpacityFactor
   {
      get => _hoverOpacityFactor;
      set => SetNotifyProperty(ref _hoverOpacityFactor, value);
   }

   [Description("The color of the border when hovering over a map element.")]
   [DefaultValue(typeof(Color), "Transparent")]
   public Color HoverBorderColor
   {
      get => _hoverBorderColor;
      set => SetNotifyProperty(ref _hoverBorderColor, value);
   }

   [Description("The color of the border when highlighting a map element.")]
   [DefaultValue(typeof(Color), "Yellow")]
   public Color HighlightBorderColor
   {
      get => _highlightBorderColor;
      set => SetNotifyProperty(ref _highlightBorderColor, value);
   }

   [Description("The color of the border when selecting a map element.")]
   [DefaultValue(typeof(Color), "Red")]
   public Color SelectedBorderColor
   {
      get => _selectedBorderColor;
      set => SetNotifyProperty(ref _selectedBorderColor, value);
   }

   [Description("The type of border to use for a selected location.")]
   [DefaultValue(BorderType.Simple)]
   public BorderType SelectedBorderType
   {
      get => _selectedBorderType;
      set => SetNotifyProperty(ref _selectedBorderType, value);
   }

   [Description("The type of border to use for a hovered location.")]
   [DefaultValue(BorderType.Wide)]
   public BorderType HoverBorderType
   {
      get => _hoverBorderType;
      set => SetNotifyProperty(ref _hoverBorderType, value);
   }

   [Description("The type of border to use for a highlighted location.")]
   [DefaultValue(BorderType.Dotted)]
   public BorderType HighlightBorderType
   {
      get => _highlightBorderType;
      set => SetNotifyProperty(ref _highlightBorderType, value);
   }

   [Description("If the border of a selected location should be animated.")]
   [DefaultValue(true)]
   public bool AnimatedSelectionBorder
   {
      get => _animatedSelectionBorder;
      set => SetNotifyProperty(ref _animatedSelectionBorder, value);
   }

   [Description("If the border of a hovered location should be animated.")]
   [DefaultValue(false)]
   public bool AnimatedHoverBorder
   {
      get => _animatedHoverBorder;
      set => SetNotifyProperty(ref _animatedHoverBorder, value);
   }

   [Description("If the border of a highlighted location should be animated.")]
   [DefaultValue(true)]
   public bool AnimatedHighlightBorder
   {
      get => _animatedHighlightBorder;
      set => SetNotifyProperty(ref _animatedHighlightBorder, value);
   }
}