using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.NUI.Headers;
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

      var header = NavigationHeader(titleBinding, target.Navigations, navHistory.Root, target);
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
         FrameworkElement  element;
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
            element = BuildCollectionOrDefaultView(navHistory.Root, type, target, nxProp, baseGrid);
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

      var headerBlock = NavigationHeader(null, target.Navigations, root, target, target.GetType().Name);
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
         FrameworkElement  element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type))
         {
            INUI value = null!;
            Nx.ForceGet(target, nxProp, ref value);
            element = GenerateShortInfo(value, root);
         }
         else
         {
            element = BuildCollectionOrDefaultView(root, type, target, nxProp, baseGrid, 6);
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
      var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, MinHeight = 20};
      object headerValue = null!;
      Nx.ForceGet(value, value.Settings.Title, ref headerValue);
      var sInfo = string.Empty;
      foreach (var nxProp in value.Settings.ShortInfoFields)
      {
         if (sInfo.Length > 0 && !sInfo.EndsWith(", "))
            sInfo += ", ";

         var type = Nx.TypeOf(value, nxProp);
         var itemType = GetCollectionItemType(type);
         var arrayType = GetArrayItemType(type);
         if (itemType != null && typeof(INUI).IsAssignableFrom(itemType))
         {
            IReadOnlyList<INUI> collection = null!;
            Nx.ForceGet(value, nxProp, ref collection);
            sInfo += $"{nxProp}:{collection.Count}";
         }
         else if (arrayType != null && typeof(INUI).IsAssignableFrom(arrayType))
         {
            INUI[] array = null!;
            Nx.ForceGet(value, nxProp, ref array);
            sInfo += $"{nxProp}:{array.Length}";
         }
         else
         {
            object propValue = null!;
            Nx.ForceGet(value, nxProp, ref propValue);
            sInfo += $"{propValue}";
         }
      }

      var fontSize = 11;
      var headerBlock = NavigationHeader(null, value.Navigations, root, value, value.GetType().Name, fontSize, FontWeights.Normal);
      headerBlock.Margin = new(6, 0, 0, 0);
      var infoBlock = new TextBlock
      {
         Text = sInfo,
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

   private static Grid GetDefaultGrid(INUI target, Enum nxProp, int leftMargin = 0)
   {
      UIElement element;
      var desc = DescriptorBlock(nxProp);
      desc.Margin = new(leftMargin, 0, 0, 0);
      switch (Nx.TypeOf(target, nxProp))
      {
         case var t when t == typeof(string):
         case var f when f == typeof(float):
         case var i when i == typeof(int):
         case var o when o == typeof(object):
         case var b when b == typeof(bool):
         case { IsClass: true }:
            var textBox = new CorneredTextBox
            {
               Height = 23,
               FontSize = 12,
               Margin = new(0),
               BorderThickness = new(1),
            };
            textBox.SetBinding(TextBox.TextProperty, GetTwoWayBinding(target, nxProp));
            element = textBox;
            break;
         default:
            throw
               new NotSupportedException($"Type {Nx.TypeOf(target, nxProp)} is not supported for property {nxProp}.");
      }

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

   private static TextBlock NavigationHeader<T>(Binding? headerBinding,
                                                INUINavigation[] navigations,
                                                ContentPresenter root,
                                                T value,
                                                string text = "",
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
      if (headerBinding != null)
         header.SetBinding(TextBlock.TextProperty, headerBinding);
      else
         header.Text = text;

      header.MouseUp += (sender, e) =>
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
         {
            root.Content = GenerateView(new(value, true, root));
         }
      };
      header.Cursor = Cursors.Hand;

      return header;
   }

   private static ContextMenu GetContextMenu(INUINavigation[] navigations, ContentPresenter root)
   {
      var contextMenu = new ContextMenu();
      foreach (var navigation in navigations)
         contextMenu.Items.Add(new MenuItem
         {
            Header = navigation.ToolStripString,
            Command = new ActionCommand(() => { GenerateAndSetView(new(navigation.Target, true, root)); }),
         });

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

   private static FrameworkElement  BuildCollectionOrDefaultView(ContentPresenter root,
                                                                 Type type,
                                                                 INUI target,
                                                                 Enum nxProp,
                                                                 Grid baseGrid,
                                                                 int leftMargin = 0)
   {
      FrameworkElement  element;
      var itemType = GetCollectionItemType(type);
      var arrayType = GetArrayItemType(type);

      if (itemType != null || arrayType != null)
      {
         // 6 rows, 2 columns
         var grid = new Grid
         {
            ColumnDefinitions =
            {
               new() { Width = new(1, GridUnitType.Star) }, new() { Width = new(1, GridUnitType.Star) },
            },
            Margin = new(leftMargin, 0, 0, 0),
         };

         var count = -1;

         // Collection of INUI
         if (typeof(INUI).IsAssignableFrom(itemType))
         {
            // Row for header
            grid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
            // We show the first 5 items of the collection as a ShortInfo
            IReadOnlyCollection<INUI> collection = null!;
            Nx.ForceGet(target, nxProp, ref collection);

            foreach (var j in Enumerable.Range(0, Math.Min(5, collection.Count)))
            {
               var value = collection.ElementAt(j);
               var shortInfo = GenerateShortInfo(value, root);
               grid.RowDefinitions.Add(new() { Height = new(20, GridUnitType.Pixel) });
               grid.Children.Add(shortInfo);
               Grid.SetRow(shortInfo, j + 1);
               Grid.SetColumn(shortInfo, 0);
               Grid.SetColumnSpan(shortInfo, 2);
            }

            count = collection.Count;
            element = grid;
         }
         else if (typeof(INUI).IsAssignableFrom(arrayType))
         {
            // We show the first 5 items of the array as a ShortInfo
            INUI[] array = null!;
            Nx.ForceGet(target, nxProp, ref array);

            foreach (var j in Enumerable.Range(0, Math.Min(5, array.Length)))
            {
               var value = array[j];
               var shortInfo = GenerateShortInfo(value, root);
               grid.RowDefinitions.Add(new() { Height = new(20, GridUnitType.Pixel) });
               grid.Children.Add(shortInfo);
               Grid.SetRow(shortInfo, j + 1);
               Grid.SetColumn(shortInfo, 0);
               Grid.SetColumnSpan(shortInfo, 2);
            }

            count = array.Length;
            element = grid;
         }
         else
         {
            element = GetDefaultGrid(target, nxProp, leftMargin);
            baseGrid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
         }

         var strackPanel = new StackPanel
         {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, 0, 0, 0),
         };
         
         var nheader = new TextBlock
         {
            Text = $"{nxProp}: {count} Items",
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
         };
         

         var openButton = GetEyeButton();
         openButton.Margin = new(4, 0, 0, 0);

         openButton.Click += (sender, e) =>
         {
            // TODO open proper collection editor for any kind of object
         };

         strackPanel.Children.Add(nheader);
         strackPanel.Children.Add(openButton);
         
         grid.Children.Add(strackPanel);
         Grid.SetRow(strackPanel, 0);
         Grid.SetColumn(strackPanel, 0);
         Grid.SetColumnSpan(strackPanel, 2);

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
      }
      else
      {
         element = GetDefaultGrid(target, nxProp, leftMargin);
      }

      return element;
   }

   private static Rectangle GetSeparatorLine(DoubleCollection dashes)
   {
      return new()
      {
         Height = 1,
         Fill = Brushes.Transparent,
         Stroke = (Brush)Application.Current.FindResource("DefaultBorderColorBrush")!,
         StrokeThickness = 1,
         StrokeDashArray = dashes,
         StrokeDashCap = PenLineCap.Flat,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Bottom,
         SnapsToDevicePixels = true,
      };
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
}