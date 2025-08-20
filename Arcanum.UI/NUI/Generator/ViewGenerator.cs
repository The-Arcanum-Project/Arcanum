using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.UI.NUI.Headers;
using Common.UI.NUI;
using Nexus.Core;

namespace Arcanum.UI.NUI.Generator;

public class ViewGenerator : IViewGenerator
{
   public NUIUserControl GenerateView(INUI target,
                                             Enum title,
                                             Enum subTitle,
                                             Enum[] nxProps,
                                             bool generateSubViews)
   {
      var titleBinding = GetOneWayBinding(target, title);
      var subtitleBinding = GetOneWayBinding(target, subTitle);

      var baseUI = target.GetBaseUI(ViewType.View);

      var baseGrid = new Grid { RowDefinitions = { new() { Height = new(50, GridUnitType.Pixel) } } };

      var header = GetHeader(titleBinding, subtitleBinding);
      baseGrid.Children.Add(header);
      Grid.SetRow(header, 0);
      Grid.SetColumn(header, 0);

      for (var i = 0; i < nxProps.Length; i++)
      {
         var nxProp = nxProps[i];
         UIElement element;
         var type = Nx.TypeOf(target, nxProp);
         if (typeof(INUI).IsAssignableFrom(type))
         {
            if (generateSubViews)
            {
               // As this is broken we hack around it for now.
               // var value = Nx.Get<INUI>(target, nxProp);
               var value = ((Pop)target).Type;
               element = value.GetEmbeddedView(nxProps);
            }
            else
            {
               element = GetStackPanel(target, nxProp);
            }
         }
         else
         {
            element = GetStackPanel(target, nxProp);
         }

         baseGrid.RowDefinitions.Add(new() { Height = new(40, GridUnitType.Pixel) });
         baseGrid.Children.Add(element);
         Grid.SetRow(element, i + 1);
         Grid.SetColumn(element, 0);
      }

      baseUI.Content = baseGrid;
      return baseUI;
   }

   private static StackPanel GetStackPanel(INUI target, Enum nxProp)
   {
      switch (Nx.TypeOf(target, nxProp))
      {
         case var t when t == typeof(string):
            var desc = DescriptorBlock(target, nxProp);
            var textBox = new TextBox();
            textBox.SetBinding(TextBox.TextProperty, GetTwoWayBinding(target, nxProp));
            return new() { Children = { desc, textBox }, Orientation = Orientation.Horizontal };
      }

      throw new NotSupportedException($"Type {Nx.TypeOf(target, nxProp)} is not supported for property {nxProp}.");
   }

   private static TextBlock DescriptorBlock(INUI target, Enum nxProp)
   {
      var textBlock = new TextBlock { Text = $"{nxProp}: " };
      return textBlock;
   }

   private static DefaultHeader GetHeader(Binding titleBinding, Binding subtitleBinding)
   {
      var header = new DefaultHeader();
      header.TitleTextBlock.SetBinding(TextBlock.TextProperty, subtitleBinding);
      header.SubTitleTextBlock.SetBinding(TextBlock.TextProperty, titleBinding);
      return header;
   }

   private static Binding GetTwoWayBinding(INUI target, Enum property)
   {
      return new()
      {
         Source = target,
         Path = new("Item[(0)]", property),
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
      };
   }

   private static Binding GetOneWayBinding(INUI target, Enum property)
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