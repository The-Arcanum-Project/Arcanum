using System.Collections;
using System.Numerics;
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
using Arcanum.UI.NUI.Generator.StructConverters;
using Common.Logger;

namespace Arcanum.UI.NUI.Generator;

// ReSharper disable once InconsistentNaming
public static class NEF
{
   private const string UI_ASSEMBLY_NAME = "Arcanum_UI";
   private const string UI_PACK_PATH = $"pack://application:,,,/{UI_ASSEMBLY_NAME};component/";
   private const string ICONS_PATH = $"{UI_PACK_PATH}Assets/Icons/";
   private static readonly BitmapImage EyeIcon = LoadBitmap($"{ICONS_PATH}20x20/Eye20x20.png");
   private static readonly BitmapImage AddIcon = LoadBitmap($"{ICONS_PATH}20x20/Add20x20.png");
   private static readonly BitmapImage SubtractIcon = LoadBitmap($"{ICONS_PATH}20x20/Minimize20x20.png");
   private static readonly BitmapImage InferMapIcon = LoadBitmap($"{ICONS_PATH}20x20/InferMap20x20.png");

   private static readonly Brush DefaultBorderBrush = GetFrozenBrush("DefaultBorderColorBrush");
   private static readonly Brush SelectedBackBrush = GetFrozenBrush("SelectedBackColorBrush");

   private static readonly FontFamily MonospacedFont =
      (FontFamily)Application.Current.FindResource("DefaultMonospacedFont")!;

   internal static BitmapImage LoadBitmap(string path)
   {
      var bmp = new BitmapImage();
      bmp.BeginInit();
      bmp.UriSource = new(path, UriKind.Absolute);
      bmp.CacheOption = BitmapCacheOption.OnLoad;
      bmp.EndInit();
      bmp.Freeze();
      return bmp;
   }

   private static Brush GetFrozenBrush(string key)
   {
      var brush = (Brush)Application.Current.FindResource(key)!;
      if (brush.CanFreeze)
         brush.Freeze(); // Makes rendering faster and thread-safe
      return brush;
   }

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
            Source = EyeIcon, Stretch = Stretch.UniformToFill,
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
            Source = AddIcon, Stretch = Stretch.UniformToFill,
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
            Source = AddIcon, Stretch = Stretch.UniformToFill,
         },
      };
   }

   public static BaseButton CreateRemoveButton()
   {
      var removeButton = new BaseButton
      {
         Content = new Image
         {
            Source = SubtractIcon, Stretch = Stretch.UniformToFill,
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
            Source = InferMapIcon, Stretch = Stretch.UniformToFill,
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
            Source = AddIcon, Stretch = Stretch.UniformToFill,
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
         BorderBrush = SelectedBackBrush,
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
      Stroke = SelectedBackBrush,
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
         InnerBorderBrush = DefaultBorderBrush,
         MinValue = float.MinValue,
         MaxValue = float.MaxValue,
         Value = value,
         StepSize = 0.1f,
         FontFamily = MonospacedFont,
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
         FontFamily = MonospacedFont,
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

      textBox.Unloaded += OnTextBoxOnUnloaded;

      return textBox;

      void OnTextBoxOnUnloaded(object o, RoutedEventArgs routedEventArgs)
      {
         textBox.DebouncedTextChanged -= debouncedHandler;
         textBox.Unloaded -= OnTextBoxOnUnloaded;
      }
   }

   public static JominiDateTextBox GetJominiDateUI(Binding binding)
   {
      var textBox = new JominiDateTextBox
      {
         Margin = new(0),
         BorderThickness = new(1, 1, 1, 1),
         FontFamily = MonospacedFont,
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
         FontFamily = MonospacedFont,
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
         FontFamily = MonospacedFont,
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
         InnerBorderBrush = DefaultBorderBrush,
         MinValue = int.MinValue,
         MaxValue = int.MaxValue,
         FontFamily = MonospacedFont,
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
         InnerBorderBrush = DefaultBorderBrush,
         MinValue = decimal.MinValue,
         MaxValue = decimal.MaxValue,
         StepSize = new(0.1),
         FontFamily = MonospacedFont,
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
            FontSize = fontSize, FontFamily = MonospacedFont,
         },
         Height = height,
         Margin = new(0),
      };
      jomColView.SetBinding(JominiColorView.ColorProperty, binding);

      return jomColView;
   }

   #endregion

   public static Grid GetVector2UI(Binding binding, bool isPropertyReadOnly, Vector2 vec2)
   {
      return BuildMultiComponentUI(binding,
                                   isPropertyReadOnly,
                                   components: [("X", vec2.X, new Vector2ComponentConverter("X")), ("Y", vec2.Y, new Vector2ComponentConverter("Y"))],
                                   factory: values => new Vector2(values[0], values[1]));
   }

   public static Grid GetVector3UI(Binding binding, bool isPropertyReadOnly, Vector3 vec3)
   {
      return BuildMultiComponentUI(binding,
                                   isPropertyReadOnly,
                                   components:
                                   [
                                      ("X", vec3.X, new Vector3ComponentConverter("X")), ("Y", vec3.Y, new Vector3ComponentConverter("Y")),
                                      ("Z", vec3.Z, new Vector3ComponentConverter("Z"))
                                   ],
                                   factory: values => new Vector3(values[0], values[1], values[2]));
   }

   public static Grid GetQuaternionUI(Binding binding, bool isPropertyReadOnly, Quaternion quat)
   {
      return BuildMultiComponentUI(binding,
                                   isPropertyReadOnly,
                                   components:
                                   [
                                      ("X", quat.X, new QuaternionComponentConverter("X")), ("Y", quat.Y, new QuaternionComponentConverter("Y")),
                                      ("Z", quat.Z, new QuaternionComponentConverter("Z")), ("W", quat.W, new QuaternionComponentConverter("W"))
                                   ],
                                   factory: values => new Quaternion(values[0], values[1], values[2], values[3]));
   }

   /// <summary>
   /// Generates a Grid containing N labeled float inputs.
   /// </summary>
   /// <param name="binding">The main binding to the parent object (used for Source/Path).</param>
   /// <param name="isReadOnly">Global read-only state.</param>
   /// <param name="components">List of tuples containing (Label Text, Initial Value, Specific Converter).</param>
   /// <param name="factory">Function that takes an array of floats (in order) and returns the new Struct instance.</param>
   private static Grid BuildMultiComponentUI(Binding binding,
                                             bool isReadOnly,
                                             (string Label, float Value, IValueConverter Converter)[] components,
                                             Func<float[], object> factory)
   {
      var grid = new Grid
      {
         Margin = new(0),
         SnapsToDevicePixels = true,
         VerticalAlignment = VerticalAlignment.Center,
      };

      var inputs = new FloatNumericUpDown[components.Length];

      for (var i = 0; i < components.Length; i++)
      {
         var (label, initialValue, converter) = components[i];
         var isLast = i == components.Length - 1;

         // Define Columns (10px Label, 1* Input)
         grid.ColumnDefinitions.Add(new() { Width = new(10, GridUnitType.Pixel) });
         grid.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });

         // Add Label
         var textBlock = new TextBlock
         {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            Margin = new(0, 0, 2, 0),
            FontFamily = ControlFactory.MonoFontFamily,
         };
         textBlock.SetValue(Grid.ColumnProperty, i * 2);
         grid.Children.Add(textBlock);

         // Create Binding for this specific component
         var compBinding = new Binding(binding.Path.Path)
         {
            Source = binding.Source,
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = converter
         };

         // Add Numeric Input
         var input = GetFloatUI(compBinding, initialValue, ControlFactory.SHORT_INFO_ROW_HEIGHT);
         input.VerticalAlignment = VerticalAlignment.Center;
         input.NudTextBox.IsReadOnly = isReadOnly;
         input.Margin = isLast ? new(0) : new(0, 0, 4, 0); // Right margin for all except last
         input.SetValue(Grid.ColumnProperty, (i * 2) + 1);

         grid.Children.Add(input);
         inputs[i] = input;
      }

      // Setup Update Logic (Reflection write-back)
      if (isReadOnly)
         return grid;

      Action<float?> onValueChanged = _ =>
      {
         // Gather all current values
         var values = new float[inputs.Length];
         for (var j = 0; j < inputs.Length; j++)
            values[j] = inputs[j].Value ?? 0;

         // Create new struct instance using the factory
         var newObject = factory(values);

         try
         {
            var source = binding.Source;
            var propName = binding.Path.Path;
            var propInfo = source.GetType().GetProperty(propName);

            if (propInfo != null && propInfo.CanWrite)
               propInfo.SetValue(source, newObject);
         }
         catch
         {
            ArcLog.Error("NEF", $"Failed to set property '{binding.Path.Path}' value via reflection.");
         }
      };

      // Subscribe
      foreach (var input in inputs)
         input.ValueChanged += onValueChanged;

      // Cleanup
      grid.Unloaded += OnGridOnUnloaded;

      void OnGridOnUnloaded(object o, RoutedEventArgs routedEventArgs)
      {
         foreach (var input in inputs)
            input.ValueChanged -= onValueChanged;
         grid.Unloaded -= OnGridOnUnloaded;
      }

      return grid;
   }
}