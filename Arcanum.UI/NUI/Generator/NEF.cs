using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Map.MapModes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.UI.Components.Converters;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

// ReSharper disable once InconsistentNaming
public static class NEF
{
   public static AutoCompleteComboBox ObjectSelector<T>(T target, IEnumerable allItems, int index) where T : INUI
   {
      var objectSelector = new AutoCompleteComboBox
      {
         FullItemsSource = allItems,
         SelectedItem = target,
         Height = 23,
         Margin = new(1),
         Padding = new(2, 0, 2, 0),
         FontSize = 11,
         Name = $"AutoComplete_{target.GetType().Name}_{index}",
      };
      return objectSelector;
   }

   public static Grid CreateHeaderGrid<T>() where T : INUI
   {
      var headerGrid = new Grid
      {
         ColumnDefinitions =
         {
            new() { Width = new(5, GridUnitType.Star) },
            new() { Width = new(5, GridUnitType.Star) },
            new() { Width = new(20, GridUnitType.Auto) },
            new() { Width = new(20, GridUnitType.Auto) },
         },
      };
      return headerGrid;
   }

   public static Grid CreateDefaultGrid(int leftMargin)
   {
      var grid = new Grid
      {
         ColumnDefinitions =
         {
            new() { Width = new(1, GridUnitType.Star) }, new() { Width = new(1, GridUnitType.Star) },
         },
         Margin = new(leftMargin, 0, 0, 0),
      };
      return grid;
   }

   public static TextBlock CreateInfoTextBlock(Enum nxProp, int leftMargin)
   {
      var info = new TextBlock
      {
         Text = $"{nxProp}: (Cannot edit collections with multiple objects selected)",
         FontStyle = FontStyles.Italic,
         Margin = new(leftMargin, 4, 0, 4)
      };
      return info;
   }

   public static DockPanel PropertyTitlePanel(int leftMargin)
   {
      return new()
      {
         LastChildFill = false,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         Margin = new(leftMargin, 0, 0, 0),
         Height = double.NaN,
      };
   }

   public static Grid CreateGridForProperty()
   {
      var grid = new Grid
      {
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Top,
         RowDefinitions = { new() { Height = new(25, GridUnitType.Pixel) } },
         ColumnDefinitions =
         {
            new() { Width = new(4, GridUnitType.Star) }, new() { Width = new(6, GridUnitType.Star) },
         },
      };
      return grid;
   }

   public static Rectangle CreateDashedBorderRectangle()
   {
      var rect = new Rectangle
      {
         Width = 1,
         Fill = Brushes.Transparent,
         Stroke = (Brush)Application.Current.FindResource("DefaultBorderColorBrush")!,
         StrokeThickness = 2,
         StrokeDashArray = new([4, 6]),
         StrokeDashCap = PenLineCap.Flat,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Stretch,
         SnapsToDevicePixels = true,
      };
      return rect;
   }

   public static Grid CreateHeaderGrid()
   {
      var headerGrid = new Grid
      {
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         ColumnDefinitions =
         {
            new() { Width = new(7, GridUnitType.Star) }, new() { Width = new(3, GridUnitType.Star) },
         },
      };
      return headerGrid;
   }

   public static BaseButton GetEyeButton()
   {
      return new()
      {
         Margin = new(2),
         Height = 20,
         Width = 20,
         HorizontalAlignment = HorizontalAlignment.Left,
         BorderThickness = new(1),
         Content = new Image
         {
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/Eye20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
      };
   }

   public static BaseButton GetCollapseButton(bool isExpanded)
   {
      const string arrowPathData = "M0,0 L0,2 L4,6 L8,2 L8,0 L4,4 z";
      var path = new Path
      {
         Data = Geometry.Parse(arrowPathData),
         Fill = (Brush)Application.Current.FindResource("DefaultForeColorBrush")!,
         Stretch = Stretch.None,
         HorizontalAlignment = HorizontalAlignment.Center,
         VerticalAlignment = VerticalAlignment.Center,
         LayoutTransform = new RotateTransform
         {
            Angle = isExpanded ? 180 : 0,
            CenterX = 4,
            CenterY = 3,
         },
      };

      return new()
      {
         Content = path,
         ToolTip = "Collapse/Expand",
         Margin = new(4, 0, 2, 0),
         Width = 20,
         Height = 20,
         BorderThickness = new(1),
         VerticalAlignment = VerticalAlignment.Center,
      };
   }

   public static BaseButton GetSetEmptyButton(Type itemType,
                                              Enum property,
                                              INUI parent,
                                              NUINavHistory navHistory,
                                              object emptyInstance,
                                              bool enabled)
   {
      var setEmptyButton = new BaseButton
      {
         Width = 20,
         Height = 20,
         ToolTip =
            enabled
               ? $"Clear {property}:\n\tSet '{property}' to '{itemType.Name}.Empty'"
               : $"{property} cannot be set to empty",
         Content = new Image
         {
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/Close20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
         BorderThickness = new(1),
         Margin = new(1, 1, -1, 1),
         Background = enabled ? Brushes.Transparent : (Brush)Application.Current.FindResource("DimErrorRedColorBrush")!,
      };

      if (enabled)
         setEmptyButton.Click += (_, _) =>
         {
            Nx.ForceSet(emptyInstance, parent, property);
            NUIViewGenerator.GenerateAndSetView(navHistory);
         };

      return setEmptyButton;
   }

   public static BaseButton CreateRemoveButton()
   {
      var removeButton = new BaseButton
      {
         Content = new Image
         {
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/Minimize20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
         ToolTip = "Remove inferred items from map selection",
         Margin = new(1),
         Width = 20,
         Height = 20,
         BorderThickness = new(1),
      };
      return removeButton;
   }

   public static BaseButton CreateSetButton()
   {
      var setButton = new BaseButton
      {
         Content = "S",
         ToolTip = "Set item from inferred list (chooses first)",
         Margin = new(1),
         Width = 20,
         Height = 20,
         BorderThickness = new(1),
      };
      return setButton;
   }

   public static BaseButton CreateAddButton()
   {
      var addButton = new BaseButton
      {
         Content = new Image
         {
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/Add20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
         ToolTip = "Add inferred items from map selection",
         Margin = new(1),
         Width = 20,
         Height = 20,
         BorderThickness = new(1),
      };
      return addButton;
   }

   public static Border EmbedMarker(Grid baseGrid)
   {
      var embedMarker = GetEmbedBorder();
      embedMarker.BorderBrush = Brushes.Purple;
      baseGrid.Children.Add(embedMarker);
      Grid.SetRow(embedMarker, 0);
      Grid.SetColumn(embedMarker, 0);
      return embedMarker;
   }

   public static void CreateSimpleHeaderGrid(TextBlock headerBlock, BaseButton collapseButton, Grid baseGrid)
   {
      var simpleHeaderGrid = new Grid
      {
         ColumnDefinitions =
         {
            new() { Width = new(1, GridUnitType.Star) }, new() { Width = new(20, GridUnitType.Auto) },
         },
      };

      headerBlock.Margin = new(6, 0, 0, 0);
      simpleHeaderGrid.Children.Add(headerBlock);
      Grid.SetColumn(headerBlock, 0);

      simpleHeaderGrid.Children.Add(collapseButton);
      Grid.SetColumn(collapseButton, 1);

      baseGrid.Children.Add(simpleHeaderGrid);
      Grid.SetRow(simpleHeaderGrid, 0);
      Grid.SetColumn(simpleHeaderGrid, 0);
   }

   public static void AddSpacerToGrid(Grid baseGrid, Enum[] embeddedFields, List<FrameworkElement> collapsibleElements)
   {
      var spacer = new Border { Height = 4 };
      baseGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
      baseGrid.Children.Add(spacer);
      Grid.SetRow(spacer, embeddedFields.Length + 1);
      Grid.SetColumn(spacer, 0);
      collapsibleElements.Add(spacer);
   }

   public static BaseButton CreateMapModeButton(IMapMode mapMode)
   {
      var mapModeButton = new BaseButton
      {
         Content = "M",
         ToolTip = $"Set to '{mapMode.Name}' Map Mode",
         Margin = new(1),
         Width = 20,
         Height = 20,
         BorderThickness = new(1),
      };
      return mapModeButton;
   }

   public static Border GetEmbedBorder()
   {
      return new()
      {
         Name = "EmbedMarker",
         Background = Brushes.Transparent,
         BorderBrush = (Brush)Application.Current.FindResource("SelectedBackColorBrush")!,
         BorderThickness = new(1, 1, 0, 1),
         CornerRadius = new(3, 0, 0, 3),
         Margin = new(0, 0, 2, 0),
         HorizontalAlignment = HorizontalAlignment.Stretch,
         IsHitTestVisible = false,
      };
   }

   public static TextBlock DescriptorBlock(Enum nxProp)
   {
      var textBlock = new TextBlock { Text = $"{nxProp}: ", VerticalAlignment = VerticalAlignment.Center };
      return textBlock;
   }

   public static TextBlock CreateHeaderTextBlock<T>(int fontSize, FontWeight? fontWeight, int height) where T : INUI
   {
      var header = new TextBlock
      {
         Background = Brushes.Transparent,
         VerticalAlignment = VerticalAlignment.Center,
         FontWeight = fontWeight ?? FontWeights.Bold,
         Margin = new(4, 0, 0, 0),
         FontSize = fontSize,
         Height = height,
         Foreground = (Brush)Application.Current.FindResource("BlueAccentColorBrush")!,
      };
      return header;
   }

   public static TextBlock CreateDashBlock<T>(int fontSize) where T : INUI
   {
      var dashBlock = new TextBlock
      {
         Text = " — ",
         VerticalAlignment = VerticalAlignment.Center,
         FontSize = fontSize,
         Height = fontSize + 4,
      };
      return dashBlock;
   }

   public static TextBlock CreateShortInfoBlock<T>(List<string> shortInfoParts, int fontSize) where T : INUI
   {
      var infoBlock = new TextBlock
      {
         Text = string.Join(", ", shortInfoParts),
         TextTrimming = TextTrimming.CharacterEllipsis,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         FontSize = fontSize,
         Height = fontSize + 4,
      };
      return infoBlock;
   }

   public static StackPanel CreateHeaderStackPanel()
   {
      var headerStack = new StackPanel
      {
         Orientation = Orientation.Horizontal,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
      };
      return headerStack;
   }

   public static TextBlock CreateItemsCountTextBlock(Enum nxProp, IList modifiableList)
   {
      var textBlock = new TextBlock
      {
         Text = $"{nxProp}: {modifiableList.Count} Items",
         FontWeight = FontWeights.Bold,
         VerticalAlignment = VerticalAlignment.Center,
         FontSize = 14,
      };
      return textBlock;
   }

   #region UI Element Generation (Low-Level Controls)

   public static readonly MultiSelectBooleanConverter MultiSelectBoolConverter = new();
   public static readonly DoubleToDecimalConverter DoubleToDecimalConverter = new();
   public static readonly EnumConverter EnumConverter = new();

   public static Rectangle GenerateDashedLine(int leftMargin) => new()
   {
      Height = 1,
      Fill = Brushes.Transparent,
      Stroke = (Brush)Application.Current.FindResource("SelectedBackColorBrush")!,
      StrokeThickness = 1,
      StrokeDashArray = new([4, 6]),
      StrokeDashCap = PenLineCap.Flat,
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Bottom,
      SnapsToDevicePixels = true,
      Margin = new(leftMargin, 0, 5, 3),
   };

   public static FloatNumericUpDown GetFloatUI(Binding binding, int height = 23, int fontSize = 12)
   {
      FloatNumericUpDown numericUpDown = new()
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         InnerBorderThickness = new(1),
         InnerBorderBrush = (Brush)Application.Current.FindResource("DefaultBorderColorBrush")!,
         MinValue = float.MinValue,
         MaxValue = float.MaxValue,
         StepSize = 0.1f,
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      numericUpDown.SetBinding(FloatNumericUpDown.ValueProperty, binding);
      return numericUpDown;
   }

   public static UserControl GetModValInstanceUI(ModValInstance instance, int height = 23, int fontSize = 12)
   {
      ModValViewer viewer = new(instance)
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      return viewer;
   }

   public static CorneredTextBox GetStringUI(Binding binding, int height = 23, int fontSize = 12)
   {
      var textBox = new CorneredTextBox
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         BorderThickness = new(1, 1, 1, 1),
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
         VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
         TextWrapping = TextWrapping.NoWrap,
      };
      textBox.SetBinding(TextBox.TextProperty, binding);
      return textBox;
   }

   public static CheckBox GetBoolUI(Binding binding, int height = 23, int fontSize = 12)
   {
      binding.Converter = MultiSelectBoolConverter;
      var checkBox = new CheckBox
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         VerticalAlignment = VerticalAlignment.Center,
         IsThreeState = true,
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);
      return checkBox;
   }

   public static AutoCompleteComboBox GetEnumUI(Type enumType, Binding binding, int height = 23, int fontSize = 12)
   {
      binding.Converter = EnumConverter;

      var comboBox = new AutoCompleteComboBox
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         BorderThickness = new(1),
         FullItemsSource = Enum.GetValues(enumType),
         IsDropdownOnly = true,
         IsReadOnly = true,
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      comboBox.SetBinding(Selector.SelectedItemProperty, binding);
      return comboBox;
   }

   public static BaseNumericUpDown GetIntUI(Binding binding, int height = 23, int fontSize = 12)
   {
      BaseNumericUpDown numericUpDown = new()
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         InnerBorderThickness = new(1, 1, 1, 1),
         InnerBorderBrush = (Brush)Application.Current.FindResource("DefaultBorderColorBrush")!,
         MinValue = int.MinValue,
         MaxValue = int.MaxValue,
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      numericUpDown.SetBinding(BaseNumericUpDown.ValueProperty, binding);
      return numericUpDown;
   }

   public static DecimalBaseNumericUpDown GetDoubleUI(Binding binding, int height = 23, int fontSize = 12)
   {
      binding.Converter = DoubleToDecimalConverter;
      DecimalBaseNumericUpDown numericUpDown = new()
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         InnerBorderThickness = new(1, 1, 1, 1),
         InnerBorderBrush = (Brush)Application.Current.FindResource("DefaultBorderColorBrush")!,
         MinValue = decimal.MinValue,
         MaxValue = decimal.MaxValue,
         StepSize = new(0.1),
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      numericUpDown.SetBinding(DecimalBaseNumericUpDown.ValueProperty, binding);
      return numericUpDown;
   }

   public static JominiColorView GetJominiColorUI(Binding binding,
                                                  JominiColor color,
                                                  int height = 23,
                                                  int fontSize = 12)
   {
      var jomColView = new JominiColorView(color)
      {
         ColorTextBlock =
         {
            FontSize = fontSize, FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
         },
      };
      jomColView.ColorTextBlock.SetBinding(TextBlock.TextProperty, binding);
      jomColView.Height = height;
      jomColView.Margin = new(0);
      return jomColView;
   }

   #endregion
}