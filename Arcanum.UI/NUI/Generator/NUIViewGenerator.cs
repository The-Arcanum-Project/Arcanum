using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.NUI.Headers;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Microsoft.Xaml.Behaviors.Core;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

public static class NUIViewGenerator
{
   private static int index = 0;

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

      var baseUI = new BaseView { Name = $"{target.Settings.Title}_{index}" };

      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(50, GridUnitType.Pixel) } } };

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
            element = GetStackPanel(target, nxProp);
            baseGrid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
         }

         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      baseUI.BaseViewBorder.Child = baseGrid;
      index++;
      return baseUI;
   }

   public static UserControl GetEmbeddedView<T>(T target,
                                                ContentPresenter root) where T : INUI
   {
      var header = target.Settings.Title;
      var embeddedFields = target.Settings.EmbeddedFields;

      var baseUI = new BaseEmbeddedView();

      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(25, GridUnitType.Pixel) } } };
      var titleBinding = GetOneWayBinding(target, header);
      var headerBlock = NavigationHeader(titleBinding, target.Navigations, root, target);
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
            element = GetStackPanel(target, nxProp);
            baseGrid.RowDefinitions.Add(new() { Height = new(25, GridUnitType.Pixel) });
         }

         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      baseUI.BaseEmbeddedBorder.Child = baseGrid;
      return baseUI;
   }

   private static StackPanel GenerateShortInfo<T>(T value, ContentPresenter root) where T : INUI
   {
      var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
      object headerValue = null!;
      Nx.ForceGet(value, value.Settings.Title, ref headerValue);
      var headerBlock = new TextBlock
      {
         Text = headerValue.ToString() ?? "Unknown", Cursor = Cursors.Hand,
      };
      var sInfo = string.Empty;
      foreach (var nxProp in value.Settings.ShortInfoFields)
      {
         if (sInfo.Length > 0)
            sInfo += ", ";
         object propValue = null!;
         Nx.ForceGet(value, nxProp, ref propValue);
         sInfo += $"{propValue}";
      }

      var infoBlock = new TextBlock
      {
         Text = sInfo, Cursor = Cursors.Hand,
      };
      stackPanel.Children.Add(headerBlock);
      stackPanel.Children.Add(infoBlock);

      headerBlock.MouseUp += (sender, e) =>
      {
         if (e.ChangedButton == MouseButton.Right)
         {
            if (value.Navigations.Length == 0)
            {
               e.Handled = true;
               return;
            }

            var contextMenu = GetContextMenu(value.Navigations, root, value);
            contextMenu.PlacementTarget = sender as UIElement ?? headerBlock;
            contextMenu.IsOpen = true;
            e.Handled = true;
         }
         else if (e.ChangedButton == MouseButton.Left)
         {
            root.Content = GenerateView(new(value, true, root));
         }
      };
      return stackPanel;
   }

   private static StackPanel GetStackPanel(INUI target, Enum nxProp)
   {
      switch (Nx.TypeOf(target, nxProp))
      {
         case var t when t == typeof(string):
         case var f when f == typeof(float):
            var desc = DescriptorBlock(nxProp);
            var textBox = new CorneredTextBox();
            textBox.SetBinding(TextBox.TextProperty, GetTwoWayBinding(target, nxProp));
            return new() { Children = { desc, textBox }, Orientation = Orientation.Horizontal };
      }

      throw new NotSupportedException($"Type {Nx.TypeOf(target, nxProp)} is not supported for property {nxProp}.");
   }

   private static TextBlock DescriptorBlock(Enum nxProp)
   {
      var textBlock = new TextBlock { Text = $"{nxProp}: " };
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

            var contextMenu = GetContextMenu(navigations, root, value);
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

   private static ContextMenu GetContextMenu<T>(INUINavigation[] navigations, ContentPresenter root, T target)
      where T : INUI
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

            var contextMenu = GetContextMenu(navigations, root, target);
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
}