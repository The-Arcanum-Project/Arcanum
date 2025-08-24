using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Microsoft.Xaml.Behaviors.Core;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

public static class NUIViewGenerator
{
   private static int _index;

   public static void GenerateAndSetView(NUINavHistory navHistory)
   {
      var view = GenerateView(navHistory);
      navHistory.Root.Content = view;
   }

   public static UserControl GenerateView(NUINavHistory navHistory)
   {
      var target = navHistory.Target;
      var titleBinding = GetOneWayBinding(target, target.Settings.Title);

      var baseUI = new BaseView
      {
         Name = $"{target.Settings.Title}_{_index}", BaseViewBorder = { BorderThickness = new(0) },
      };

      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(40, GridUnitType.Pixel) } }, Margin = new(4) };

      var header = NavigationHeader(target.Navigations, navHistory.Root, target);
      header.FontSize = 24;
      header.Height = 32;
      header.HorizontalAlignment = HorizontalAlignment.Center;
      header.VerticalAlignment = VerticalAlignment.Top;
      baseGrid.Children.Add(header);
      Grid.SetRow(header, 0);
      Grid.SetColumn(header, 0);

      for (var i = 0; i < target.Settings.ViewFields.Length; i++)
      {
         var nxProp = target.Settings.ViewFields[i];
         FrameworkElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type) || typeof(INUI) == type)
         {
            // Detect if value has ref to target. --> 1 to n relationship.
            if (navHistory.GenerateSubViews)
            {
               INUI value = null!;
               Nx.ForceGet(target, nxProp, ref value);
               element = GetEmbeddedView(value, navHistory.Root);
            }
            else
            {
               element = GenerateShortInfo(target, navHistory.Root);
            }
         }
         else
         {
            element = BuildCollectionOrDefaultView(navHistory.Root, type, target, nxProp);
         }
         
         element.VerticalAlignment = VerticalAlignment.Stretch;
         baseGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      baseUI.BaseViewBorder.Child = baseGrid;
      _index++;
      return baseUI;
   }

   private static BaseEmbeddedView GetEmbeddedView<T>(T target,
                                                      ContentPresenter root) where T : INUI
   {
      var embeddedFields = target.Settings.EmbeddedFields;

      var baseUI = new BaseEmbeddedView();
      var baseGrid = baseUI.ContentGrid;

      var headerBlock = NavigationHeader(target.Navigations, root, target, target.GetType().Name);
      headerBlock.Margin = new(6, 0, 0, 0);
      baseGrid.RowDefinitions.Add(new() { Height = new(headerBlock.Height, GridUnitType.Pixel) });
      baseGrid.Children.Add(headerBlock);
      Grid.SetRow(headerBlock, 0);
      Grid.SetColumn(headerBlock, 0);

      var embedMarker = GetEmbedBorder();
      embedMarker.BorderBrush = Brushes.Purple;
      baseGrid.Children.Add(embedMarker);
      Grid.SetRow(embedMarker, 0);
      Grid.SetColumn(embedMarker, 0);

      for (var i = 0; i < embeddedFields.Length; i++)
      {
         var nxProp = embeddedFields[i];
         FrameworkElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type))
         {
            INUI value = null!;
            Nx.ForceGet(target, nxProp, ref value);
            if (value == null!)
               continue;
            element = GenerateShortInfo(value, root);
         }
         else
         {
            element = BuildCollectionOrDefaultView(root, type, target, nxProp, 6);
         }

         baseGrid.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Auto) });
         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      baseGrid.RowDefinitions.Add(new() { Height = new(4, GridUnitType.Pixel) });
      Grid.SetRowSpan(embedMarker, baseGrid.RowDefinitions.Count);
      return baseUI;
   }

   private static StackPanel GenerateShortInfo<T>(T value, ContentPresenter root) where T : INUI
   {
      var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, MinHeight = 20 };
      object headerValue = null!;
      Nx.ForceGet(value, value.Settings.Title, ref headerValue);
      var shortInfoParts = new List<string>();
      foreach (var nxProp in value.Settings.ShortInfoFields)
      {
         object pVal = null!;
         Nx.ForceGet(value, nxProp, ref pVal);
         if (pVal is IEnumerable collection and not string)
         {
            var count = collection.Cast<object>().Count();
            shortInfoParts.Add($"{nxProp}: {count}");
         }
         else
         {
            shortInfoParts.Add($"{GetDisplayString(pVal)}");
         }
      }

      const int fontSize = 11;
      var headerBlock = NavigationHeader(value.Navigations,
                                         root,
                                         value,
                                         value.GetType().Name,
                                         fontSize,
                                         FontWeights.Normal);
      headerBlock.Margin = new(6, 0, 0, 0);
      var infoBlock = new TextBlock
      {
         Text = string.Join(", ", shortInfoParts),
         TextTrimming = TextTrimming.CharacterEllipsis,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Center,
         FontSize = fontSize,
         Height = fontSize + 4,
      };
      var dashBlock = new TextBlock
      {
         Text = " — ",
         VerticalAlignment = VerticalAlignment.Center,
         FontSize = fontSize,
         Height = fontSize + 4,
      };
      stackPanel.Children.Add(headerBlock);
      stackPanel.Children.Add(dashBlock);
      stackPanel.Children.Add(infoBlock);

      return stackPanel;
   }

   private static Grid GetTypeSpecificGrid(INUI target, Enum nxProp, int leftMargin = 0)
   {
      var type = Nx.TypeOf(target, nxProp);
      var binding = GetTwoWayBinding(target, nxProp);
      Control element;
      // Fallback to existing logic
      if (type == typeof(float))
         element = GetFloatUI(binding);
      else if (type == typeof(string))
         element = GetStringUI(binding);
      else if (type == typeof(bool))
         element = GetBoolUI(binding);
      else if (type.IsEnum)
         element = GetEnumUI(type, binding);
      else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
         element = GetIntUI(binding);
      else if (type == typeof(double) || type == typeof(decimal))
         element = GetDoubleUI(binding);
      else
         throw new NotSupportedException($"Type {type} is not supported for property {nxProp}.");

      element.IsEnabled = !target.IsReadonly;
      element.VerticalAlignment = VerticalAlignment.Stretch;

      var desc = DescriptorBlock(nxProp);
      desc.Margin = new(leftMargin, 0, 0, 0);

      var line = new Rectangle
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
      RenderOptions.SetEdgeMode(line, EdgeMode.Aliased);

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

      grid.Children.Add(desc);
      Grid.SetRow(desc, 0);
      Grid.SetColumn(desc, 0);

      grid.Children.Add(line);
      Grid.SetRow(line, 0);
      Grid.SetColumn(line, 0);

      grid.Children.Add(element);
      Grid.SetRow(element, 0);
      Grid.SetColumn(element, 1);

      return grid;
   }

   private static TextBlock DescriptorBlock(Enum nxProp)
   {
      var textBlock = new TextBlock { Text = $"{nxProp}: ", VerticalAlignment = VerticalAlignment.Center };
      return textBlock;
   }

   private static TextBlock NavigationHeader<T>(INUINavigation[] navigations,
                                                ContentPresenter root,
                                                T value,
                                                string text = null!,
                                                int fontSize = 16,
                                                FontWeight? fontWeight = null) where T : INUI
   {
      var height = fontSize + 4;
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
      if (string.IsNullOrWhiteSpace(text))
      {
         object headerValue = null!;
         Nx.ForceGet(value, value.Settings.Title, ref headerValue);
         text = GetDisplayString(headerValue);
      }
      header.Text = text;

      MouseButtonEventHandler clickHandler = (sender, e) =>
      {
         if (e.ChangedButton == MouseButton.Right)
         {
            if (navigations.Length == 0)
            {
               e.Handled = true;
               return;
            }

            var contextMenu = GetContextMenu(navigations, root);
            contextMenu.PlacementTarget = sender as UIElement ?? header;
            contextMenu.IsOpen = true;
            e.Handled = true;
         }
         else if (e.ChangedButton == MouseButton.Left)
            root.Content = GenerateView(new(value, true, root));
      };
      header.MouseUp += clickHandler;
      header.Unloaded += (_, _) => { header.MouseUp -= clickHandler; };
      header.Cursor = Cursors.Hand;

      return header;
   }

   /// <summary>
   /// Define the context menu for navigation options.
   /// Use <c>null</c> as a value in the <see cref="navigations"/> array to create a separator.
   /// </summary>
   /// <param name="navigations"></param>
   /// <param name="root"></param>
   /// <returns></returns>
   private static ContextMenu GetContextMenu(INUINavigation?[] navigations, ContentPresenter root)
   {
      var contextMenu = new ContextMenu();
      foreach (var navigation in navigations)
      {
         if (navigation == null)
         {
            contextMenu.Items.Add(new Separator());
            continue;
         }

         contextMenu.Items.Add(new MenuItem
         {
            FontSize = 12,
            FontWeight = FontWeights.Normal,
            Foreground = (Brush)Application.Current.FindResource("DefaultForeColorBrush")!,
            Header = navigation.ToolStripString,
            Command = new ActionCommand(() => { GenerateAndSetView(new(navigation.Target, true, root)); }),
         });
      }

      return contextMenu;
   }

   private static Binding GetTwoWayBinding<T>(T target, Enum property) where T : INUI
   {
      return new()
      {
         Source = target,
         Path = new("Item[(0)]", property),
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      };
   }

   private static Binding GetOneWayBinding<T>(T target, Enum property) where T : INUI
   {
      return new()
      {
         Source = target,
         Path = new("Item[(0)]", property),
         Mode = BindingMode.OneWay,
         UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      };
   }

   public static Type? GetCollectionItemType(Type collectionType)
   {
      if (collectionType == typeof(string) || !collectionType.IsGenericType)
         return null;

      var enumerableInterface = collectionType.GetInterfaces()
                                              .FirstOrDefault(i => i.IsGenericType &&
                                                                   i.GetGenericTypeDefinition() ==
                                                                   typeof(IEnumerable<>));

      return enumerableInterface?.GetGenericArguments()[0];
   }

   public static Type? GetArrayItemType(Type arrayType)
   {
      if (arrayType == typeof(string) || !arrayType.IsArray)
         return null;

      return arrayType.IsArray ? arrayType.GetElementType() : null;
   }

   private static BaseButton GetEyeButton()
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
            Source = new BitmapImage(new("pack://application:,,,/Assets/Icons/20x20/Eye20x20.png")),
            Stretch = Stretch.UniformToFill,
         }
      };
   }

   private static Grid BuildCollectionOrDefaultView(ContentPresenter root,
                                                    Type type,
                                                    INUI target,
                                                    Enum nxProp,
                                                    int leftMargin = 0)
   {
      var itemType = GetCollectionItemType(type) ?? GetArrayItemType(type);

      // If it is not a collection or array, fall back to default handling.
      if (itemType == null)
      {
         return GetTypeSpecificGrid(target, nxProp, leftMargin);
      }

      object collectionObject = null!;
      Nx.ForceGet(target, nxProp, ref collectionObject);

      // Cast to the non-generic IEnumerable to handle any collection type.
      if (collectionObject is not IEnumerable collection)
      {
         return GetTypeSpecificGrid(target, nxProp, leftMargin);
      }

      // Filter the collection for items that ACTUALLY implement INUI at runtime.
      var inuiItems = collection.OfType<INUI>().ToList();


      var grid = new Grid
      {
         ColumnDefinitions =
         {
            new() { Width = new(1, GridUnitType.Star) }, new() { Width = new(1, GridUnitType.Star) },
         },
         Margin = new(leftMargin, 0, 0, 0),
      };

      grid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });

      // Displaying Concrete Types
      foreach (var item in inuiItems.Take(5))
      {
         var shortInfo = GenerateShortInfo(item, root);

         grid.RowDefinitions.Add(new() { Height = new(20, GridUnitType.Pixel) });
         grid.Children.Add(shortInfo);
         Grid.SetRow(shortInfo, grid.RowDefinitions.Count - 1);
         Grid.SetColumn(shortInfo, 0);
         Grid.SetColumnSpan(shortInfo, 2);
      }

      var stackPanel = new StackPanel
      {
         Orientation = Orientation.Horizontal,
         HorizontalAlignment = HorizontalAlignment.Left,
         VerticalAlignment = VerticalAlignment.Center,
      };

      var nheader = new TextBlock
      {
         Text = $"{nxProp}: {inuiItems.Count} Items",
         FontWeight = FontWeights.Bold,
         VerticalAlignment = VerticalAlignment.Center,
         FontSize = 14,
      };

      var openButton = GetEyeButton();
      openButton.Margin = new(4, 0, 0, 0);
      openButton.Click += (_, _) =>
      {
         // TODO open proper collection editor for any kind of object
      };

      stackPanel.Children.Add(nheader);
      stackPanel.Children.Add(openButton);

      grid.Children.Add(stackPanel);
      Grid.SetRow(stackPanel, 0);
      Grid.SetColumn(stackPanel, 0);
      Grid.SetColumnSpan(stackPanel, 2);

      // --- Decorative Border ---
      if (grid.RowDefinitions.Count > 1)
      {
         var rect = new Rectangle()
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

         grid.Children.Add(rect);
         Grid.SetRow(rect, 1);
         Grid.SetColumn(rect, 0);
         Grid.SetRowSpan(rect, grid.RowDefinitions.Count - 1);
      }

      return grid;
   }

   private static Border GetEmbedBorder()
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

   /// <summary>
   /// Gets a user-friendly string for an object. If the object has a custom
   /// ToString() override, it's used. Otherwise, the class's simple name is returned.
   /// </summary>
   private static string GetDisplayString(object? obj)
   {
      if (obj is null)
         return "null"; // Or string.Empty, depending on what you prefer for nulls

      var type = obj.GetType();

      // Get the MethodInfo for the ToString method.
      // This will get the most-derived override of ToString().
      var toStringMethod = type.GetMethod(nameof(ToString), Type.EmptyTypes);

      // If the method was declared on a type other than System.Object,
      // it means it has been overridden somewhere in the inheritance chain.
      if (toStringMethod != null && toStringMethod.DeclaringType != typeof(object))
         // It's an override, so call it.
         return obj.ToString() ?? string.Empty;

      // It's the default object.ToString(), so just return the type name.
      return $"<{type.Name}>";
   }

   #region Contorl Generators

   private static FloatNumericUpDown GetFloatUI(Binding binding, int height = 23, int fontSize = 12)
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
      };
      numericUpDown.SetBinding(FloatNumericUpDown.ValueProperty, binding);
      return numericUpDown;
   }

   private static TextBox GetStringUI(Binding binding, int height = 23, int fontSize = 12)
   {
      var textBox = new CorneredTextBox
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         BorderThickness = new(1, 1, 1, 1),
      };
      textBox.SetBinding(TextBox.TextProperty, binding);
      return textBox;
   }

   private static CheckBox GetBoolUI(Binding binding, int height = 23, int fontSize = 12)
   {
      var checkBox = new CheckBox
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         VerticalAlignment = VerticalAlignment.Center,
      };
      checkBox.SetBinding(ToggleButton.IsCheckedProperty, binding);
      return checkBox;
   }

   private static ComboBox GetEnumUI(Type enumType, Binding binding, int height = 23, int fontSize = 12)
   {
      var comboBox = new ComboBox
      {
         Height = height,
         FontSize = fontSize,
         Margin = new(0),
         BorderThickness = new(1),
         ItemsSource = Enum.GetValues(enumType),
      };
      comboBox.SetBinding(Selector.SelectedItemProperty, binding);
      return comboBox;
   }

   private static BaseNumericUpDown GetIntUI(Binding binding, int height = 23, int fontSize = 12)
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
      };
      numericUpDown.SetBinding(BaseNumericUpDown.ValueProperty, binding);
      return numericUpDown;
   }

   private static DecimalBaseNumericUpDown GetDoubleUI(Binding binding, int height = 23, int fontSize = 12)
   {
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
      };
      numericUpDown.SetBinding(DecimalBaseNumericUpDown.ValueProperty, binding);
      return numericUpDown;
   }

   #endregion
}