using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.UI.NUI.Headers;
using Arcanum.UI.NUI.UserControls.BaseControls;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

public static class NUIViewGenerator
{
   public static UserControl GenerateView<T>(T target,
                                             bool generateSubViews,
                                             ContentPresenter root) where T : INUI
   {
      var titleBinding = GetOneWayBinding(target, target.Settings.Title);
      var subtitleBinding = GetOneWayBinding(target, target.Settings.Description);

      var baseUI = new BaseView();

      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(50, GridUnitType.Pixel) } } };

      var header = GetDescHeader(titleBinding, subtitleBinding);
      baseGrid.Children.Add(header);
      Grid.SetRow(header, 0);
      Grid.SetColumn(header, 0);

      for (var i = 0; i < target.Settings.ViewFields.Length; i++)
      {
         var nxProp = target.Settings.ViewFields[i];
         UIElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type))
         {
            // Detect if value has ref to target. --> 1 to n relationship.
            if (generateSubViews)
            {
               INUI value = null!;
               Nx.ForceGet(target, nxProp, ref value);
               element = GetEmbeddedView(value, root);
               baseGrid.RowDefinitions.Add(new() { Height = new(40, GridUnitType.Auto) });
            }
            else
            {
               element = GetStackPanel(target, nxProp);
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
            element = GenerateShortInfo(value);
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

   private static UIElement GenerateShortInfo<T>(T value) where T : INUI
   {
      throw new NotImplementedException();
   }

   private static StackPanel GetStackPanel(INUI target, Enum nxProp)
   {
      switch (Nx.TypeOf(target, nxProp))
      {
         case var t when t == typeof(string):
         case var f when f == typeof(float):
            var desc = DescriptorBlock(nxProp);
            var textBox = new TextBox();
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
                                                INavigate[] navigations,
                                                ContentPresenter root,
                                                T value) where T : INUI
   {
      var header = new TextBlock();
      header.SetBinding(TextBlock.TextProperty, headerBinding);

      header.MouseUp += (sender, e) =>
      {
         if (e.ChangedButton == MouseButton.Right)
         {
            var contextMenu = new ContextMenu();
            foreach (var navigation in navigations)
               contextMenu.Items.Add(new MenuItem
               {
                  Header = navigation.ToolStripString, Command = navigation.Command,
               });

            if (contextMenu.Items.Count > 0)
            {
               contextMenu.PlacementTarget = sender as UIElement ?? header;
               contextMenu.IsOpen = true;
               e.Handled = true;
            }
         }
         else if (e.ChangedButton == MouseButton.Left)
         {
            root.Content = GenerateView(value, true, root);
         }
      };
      header.Cursor = Cursors.Hand;

      return header;
   }

   private static DefaultHeader GetDescHeader(Binding titleBinding, Binding subtitleBinding)
   {
      var header = new DefaultHeader();
      header.TitleTextBlock.SetBinding(TextBlock.TextProperty, subtitleBinding);
      header.SubTitleTextBlock.SetBinding(TextBlock.TextProperty, titleBinding);
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