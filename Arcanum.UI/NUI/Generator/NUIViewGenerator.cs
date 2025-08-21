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
      var subtitleBinding = GetOneWayBinding(target, target.Settings.Description);

      var baseUI = new BaseView
      {
         Name = $"{target.Settings.Title}_{_index}", BaseViewBorder = { BorderThickness = new(0) },
      };

      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(40, GridUnitType.Pixel) } }, Margin = new(4) };

      var header = GetDescHeader(titleBinding, subtitleBinding, target.Navigations, navHistory.Root, target);
      baseGrid.Children.Add(header);
      Grid.SetRow(header, 0);
      Grid.SetColumn(header, 0);

      for (var i = 0; i < target.Settings.ViewFields.Length; i++)
      {
         var nxProp = target.Settings.ViewFields[i];
         UIElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type) || typeof(INUI) == type)
         {
            // Detect if value has ref to target. --> 1 to n relationship.
            if (navHistory.GenerateSubViews)
            {
               INUI value = null!;
               Nx.ForceGet(target, nxProp, ref value);
               element = GetEmbeddedView(value, navHistory.Root);
               baseGrid.RowDefinitions.Add(new() { Height = new(40, GridUnitType.Auto) });
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
      var header = target.Settings.Title;
      var embeddedFields = target.Settings.EmbeddedFields;

      var baseUI = new BaseEmbeddedView();

      var baseGrid = baseUI.ContentGrid; 
      baseGrid.RowDefinitions.Add(new() { Height = new(30, GridUnitType.Pixel) });
      var titleBinding = GetOneWayBinding(target, header);
      var headerBlock = NavigationHeader(titleBinding, target.Navigations, root, target);
      headerBlock.Margin = new(6, 0, 0, 0);
      Grid.SetRow(headerBlock, 0);
      Grid.SetColumn(headerBlock, 0);
      baseGrid.Children.Add(headerBlock);

      for (var i = 0; i < embeddedFields.Length; i++)
      {
         var nxProp = embeddedFields[i];
         UIElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type))
         {
            INUI value = null!;
            Nx.ForceGet(target, nxProp, ref value);
            element = GenerateShortInfo(value, root);
            baseGrid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Auto) });
         }
         else
         {
            element = BuildCollectionOrDefaultView(root, type, target, nxProp, baseGrid, 6);
         }

         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      Grid.SetRowSpan(baseUI.EmbedMarker, baseGrid.RowDefinitions.Count);
      
      return baseUI;
   }

   private static StackPanel GenerateShortInfo<T>(T value, ContentPresenter root) where T : INUI
   {
      var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
      object headerValue = null!;
      Nx.ForceGet(value, value.Settings.Title, ref headerValue);
      var sInfo = string.Empty;
      foreach (var nxProp in value.Settings.ShortInfoFields)
      {
         if (sInfo.Length > 0)
            sInfo += ", ";

         var type = Nx.TypeOf(value, nxProp);
         var itemType = GetCollectionItemType(type);
         var arrayType = GetArrayItemType(type);
         if (itemType != null && typeof(INUI).IsAssignableFrom(itemType))
         {
            ICollection<INUI> collection = null!;
            Nx.ForceGet(value, nxProp, ref collection);
            sInfo += $"{collection.Count}:{nxProp}";
         }
         else if (arrayType != null && typeof(INUI).IsAssignableFrom(arrayType))
         {
            INUI[] array = null!;
            Nx.ForceGet(value, nxProp, ref array);
            sInfo += $"{array.Length}:{nxProp}";
         }
         else
         {
            object propValue = null!;
            Nx.ForceGet(value, nxProp, ref propValue);
            sInfo += $"{propValue}";
         }
      }

      var headerBlock = NavigationHeader(GetOneWayBinding(value, value.Settings.Title), value.Navigations, root, value);
      var infoBlock = new TextBlock
      {
         Text = sInfo, Cursor = Cursors.Hand,
      };
      stackPanel.Children.Add(headerBlock);
      stackPanel.Children.Add(infoBlock);

      return stackPanel;
   }

   private static Grid GetDefaultGrid(INUI target, Enum nxProp, int leftMargin = 0)
   {
      UIElement element;
      var desc = DescriptorBlock(nxProp);
      desc.Margin = new (leftMargin, 0, 0, 0);
      switch (Nx.TypeOf(target, nxProp))
      {
         case var t when t == typeof(string):
         case var f when f == typeof(float):
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
         Margin = new (leftMargin, 0, 5, 3),
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

   private static TextBlock NavigationHeader<T>(Binding headerBinding,
                                                INUINavigation[] navigations,
                                                ContentPresenter root,
                                                T value) where T : INUI
   {
      var header = new TextBlock();
      header.SetBinding(TextBlock.TextProperty, headerBinding);

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

   private static DefaultHeader GetDescHeader(Binding titleBinding,
                                              Binding subtitleBinding,
                                              INUINavigation[] navigations,
                                              ContentPresenter root,
                                              INUI target)
   {
      //var header = new DefaultHeader { TitleTextBlock = NavigationHeader(subtitleBinding, navigations, root, target) };
      var header = new DefaultHeader();
      header.TitleTextBlock.SetBinding(TextBlock.TextProperty, titleBinding);
      header.TitleTextBlock.MouseUp += (sender, e) =>
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
            root.Content = GenerateView(new(target, true, root));
         }
      };

      header.Cursor = Cursors.Hand;
      header.SubTitleTextBlock.SetBinding(TextBlock.TextProperty, subtitleBinding);
      return header;
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
      var enumerableInterface = collectionType.GetInterfaces()
                                              .FirstOrDefault(i => i.IsGenericType &&
                                                                   i.GetGenericTypeDefinition() ==
                                                                   typeof(IEnumerable<>));

      return enumerableInterface?.GetGenericArguments()[0];
   }

   public static Type? GetArrayItemType(Type arrayType)
   {
      return arrayType.IsArray ? arrayType.GetElementType() : null;
   }

   private static BaseButton GetEyeButton()
   {
      return new()
      {
         Margin = new(2),
         Height = 20,
         Width = 20,
         Content = new Image()
         {
            Source = new BitmapImage(new("pack://application:,,,/Assets/Icons/20x20/Eye20x20.png")),
            Stretch = Stretch.None,
         }
      };
   }

   private static UIElement BuildCollectionOrDefaultView(ContentPresenter root,
                                                         Type type,
                                                         INUI target,
                                                         Enum nxProp,
                                                         Grid baseGrid,
                                                         int leftmargin = 0)
   {
      UIElement element;
      var itemType = GetCollectionItemType(type);
      var arrayType = GetArrayItemType(type);

      if (itemType == null || arrayType == null)
      {
         // 6 rows, 2 columns
         var grid = new Grid
         {
            RowDefinitions =
            {
               new() { Height = new(25, GridUnitType.Pixel) },
               new() { Height = new(20, GridUnitType.Pixel) },
               new() { Height = new(20, GridUnitType.Pixel) },
               new() { Height = new(20, GridUnitType.Pixel) },
               new() { Height = new(20, GridUnitType.Pixel) },
            },
            ColumnDefinitions =
            {
               new() { Width = new(1, GridUnitType.Star) }, new() { Width = new(1, GridUnitType.Star) },
            },
            Margin = new(leftmargin, 0, 0, 0),
         };

         var count = -1;

         // Collection of INUI
         if (typeof(INUI).IsAssignableFrom(itemType))
         {
            // We show the first 5 items of the collection as a ShortInfo
            ICollection<INUI> collection = null!;
            Nx.ForceGet(target, nxProp, ref collection);

            foreach (var j in Enumerable.Range(0, Math.Min(5, collection.Count)))
            {
               var value = collection.ElementAt(j);
               var shortInfo = GenerateShortInfo(value, root);
               grid.Children.Add(shortInfo);
               Grid.SetRow(shortInfo, j);
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
               grid.Children.Add(shortInfo);
               Grid.SetRow(shortInfo, j);
               Grid.SetColumn(shortInfo, 0);
               Grid.SetColumnSpan(shortInfo, 2);
            }

            count = array.Length;
            element = grid;
         }
         else
         {
            element = GetDefaultGrid(target, nxProp, leftmargin);
            baseGrid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
         }

         var nheader = new TextBlock
         {
            Text = $"{nxProp}: {count} Items", FontWeight = FontWeights.Bold,
         };
         grid.Children.Add(nheader);
         Grid.SetRow(nheader, 0);
         Grid.SetColumn(nheader, 0);

         var openButton = GetEyeButton();

         openButton.Click += (sender, e) =>
         {
            // TODO open proper collection editor for any kind of object
         };

         grid.Children.Add(openButton);
         Grid.SetRow(openButton, 0);
         Grid.SetColumn(openButton, 1);
      }
      else
      {
         element = GetDefaultGrid(target, nxProp, leftmargin);
         baseGrid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
      }

      return element;
   }
}