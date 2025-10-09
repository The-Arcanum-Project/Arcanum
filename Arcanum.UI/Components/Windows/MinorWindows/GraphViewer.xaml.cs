using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.NUI.GraphDisplay;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.Registry;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class GraphViewer
{
   public static readonly DependencyProperty ObjectsOfTypeProperty =
      DependencyProperty.Register(nameof(ObjectsOfType),
                                  typeof(List<IEu5Object>),
                                  typeof(GraphViewer),
                                  new(default(List<IEu5Object>)));

   public List<IEu5Object> ObjectsOfType
   {
      get => (List<IEu5Object>)GetValue(ObjectsOfTypeProperty);
      set => SetValue(ObjectsOfTypeProperty, value);
   }

   public static readonly DependencyProperty Eu5ObjectsProperty =
      DependencyProperty.Register(nameof(Eu5Objects),
                                  typeof(List<Type>),
                                  typeof(GraphViewer),
                                  new(default(List<Type>)));

   public List<Type> Eu5Objects
   {
      get => (List<Type>)GetValue(Eu5ObjectsProperty);
      set => SetValue(Eu5ObjectsProperty, value);
   }

   public GraphViewer()
   {
      InitializeComponent();
      var types = Eu5ObjectsRegistry.Eu5Objects.ToList();
      types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
      Eu5Objects = types;

      ObjectListView.SelectedIndex = types.IndexOf(typeof(Character));
   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Type type })
         return;

      var dict = ((IEu5Object)EmptyRegistry.Empties[type]).GetGlobalItemsNonGeneric();

      var values = dict.Values.Cast<IEu5Object>().ToList();

      values.Sort((x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));

      ObjectsOfType = new(values);

      ObjectListView.SelectedIndex = 0;
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView lv || lv.SelectedItems.Count != 1)
         return;

      var selectedItem = (IEu5Object)lv.SelectedItems[0]!;
      var graph = selectedItem.CreateGraph();

      GraphLayout.ApplyGraphLayout(graph, (float)GraphCanvas.ActualWidth, (float)GraphCanvas.ActualHeight);
      graph.DrawToCanvas(GraphCanvas);
   }
}