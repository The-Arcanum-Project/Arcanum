using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.UI.Components.Converters;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;

namespace Arcanum.UI.NUI.Generator;

// ReSharper disable once InconsistentNaming
public static class NEF
{
   public static DockPanel PropertyTitlePanel(int leftMargin)
   {
      return new()
      {
         LastChildFill = false,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         Margin = new(leftMargin, 0, 0, 0),
      };
   }

   public static TextBlock InlineHeaderPanel(Enum nxProp, string type, int columnSpan)
   {
      var tb = new TextBlock
      {
         Text = $"{nxProp} ({type})",
         FontSize = 11,
         FontWeight = FontWeights.Bold,
         FontStyle = FontStyles.Italic,
         Foreground = ControlFactory.ForegroundBrush,
         Background = ControlFactory.BackColorBrush,
         VerticalAlignment = VerticalAlignment.Center,
         HorizontalAlignment = HorizontalAlignment.Center,
         Margin = new(8, 0, 8, 0),
         Padding = new(5, 0, 5, 0),
      };
      tb.SetValue(Grid.ColumnSpanProperty, columnSpan);
      return tb;
   }

   public static Border InlineBorderMarker(int leftMargin, int columnSpan)
   {
      var rect = new Border
      {
         Height = 1,
         Background = ControlFactory.ForegroundBrush,
         BorderBrush = ControlFactory.ForegroundBrush,
         CornerRadius = new(1),
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         SnapsToDevicePixels = true,
         Margin = new(leftMargin, 2, 4, 0),
      };
      rect.SetValue(Grid.ColumnSpanProperty, columnSpan);
      return rect;
   }

   public static AutoCompleteComboBox ObjectSelector<T>(T target, IEnumerable allItems, Binding binding, int index)
      where T : INUI
   {
      var objectSelector = new AutoCompleteComboBox
      {
         FullItemsSource = allItems,
         Height = 23,
         Margin = new(1),
         Padding = new(2, 0, 2, 0),
         FontSize = 11,
         Name = $"AutoComplete_{target.GetType().Name}_{index}",
      };
      objectSelector.SetBinding(Selector.SelectedItemProperty, binding);
      return objectSelector;
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

   public static BaseButton GetGraphButton()
   {
      return new()
      {
         Margin = new(2),
         Height = 20,
         Width = 20,
         BorderThickness = new(1),
         Content = new Image
         {
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/Link20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
      };
   }

   public static BaseButton GetCreateNewButton()
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
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/Add20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
      };
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
         Content = new Image
         {
            Source = new BitmapImage(new("/Arcanum_UI;component/Assets/Icons/20x20/InferMap20x20.png",
                                         UriKind.RelativeOrAbsolute)),
            Stretch = Stretch.UniformToFill,
         },
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

   public static FloatNumericUpDown GetFloatUI(Binding binding, float value, int height = 23, int fontSize = 12)
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
         Value = value,
         StepSize = 0.1f,
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
      };
      numericUpDown.SetBinding(FloatNumericUpDown.ValueProperty, binding);
      return numericUpDown;
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
         UseDebouncing = true,
      };
      binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
      textBox.SetBinding(TextBox.TextProperty, binding);

      RoutedEventHandler debouncedHandler = (_, _) =>
      {
         var expr = textBox.GetBindingExpression(TextBox.TextProperty);
         expr?.UpdateSource();
      };

      textBox.DebouncedTextChanged += debouncedHandler;

      textBox.Unloaded += (_, _) => { textBox.DebouncedTextChanged -= debouncedHandler; };

      return textBox;
   }

   public static JominiDateTextBox GetJominiDateUI(Binding binding)
   {
      var textBox = new JominiDateTextBox
      {
         Margin = new(0),
         BorderThickness = new(1, 1, 1, 1),
         FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
         VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
         TextWrapping = TextWrapping.NoWrap,
      };

      var textBinding = new Binding(binding.Path.Path)
      {
         Source = binding.Source,
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
         ValidatesOnExceptions = true,
         Converter = new JominiDateToStringConverter(),
      };

      textBox.SetBinding(TextBox.TextProperty, textBinding);

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

   public static BaseNumericUpDown GetIntUI(Binding binding, int value, int height = 23, int fontSize = 12)
   {
      BaseNumericUpDown numericUpDown = new()
      {
         Height = height,
         FontSize = fontSize,
         Value = value,
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

   public static DecimalBaseNumericUpDown GetDoubleUI(Binding binding,
                                                      decimal value,
                                                      int height = 23,
                                                      int fontSize = 12)
   {
      binding.Converter = DoubleToDecimalConverter;
      DecimalBaseNumericUpDown numericUpDown = new()
      {
         Height = height,
         FontSize = fontSize,
         Value = value,
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
                                                  bool isReadOnly,
                                                  int fontSize = 12,
                                                  int height = 23)
   {
      var jomColView = new JominiColorView(JominiColor.Empty, isReadOnly)
      {
         ColorTextBlock =
         {
            FontSize = fontSize, FontFamily = (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!,
         },
         Height = height,
         Margin = new(0),
      };
      jomColView.SetBinding(JominiColorView.ColorProperty, binding);

      return jomColView;
   }

   #endregion
}