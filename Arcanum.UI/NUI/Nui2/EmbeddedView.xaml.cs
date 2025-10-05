using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen;
using Nexus.Core;

namespace Arcanum.UI.NUI.Nui2;

public partial class EmbeddedView
{
   public PropertyEditorViewModel ViewModel { get; set; }
   private AutoCompleteComboBox Selector { get; set; }

   public EmbeddedView(PropertyEditorViewModel vm)
   {
      ViewModel = vm;
      DataContext = ViewModel;
      InitializeComponent();
      Selector = NEF.ObjectSelector(vm.Target, vm.Embedded.GetGlobalItemsNonGeneric().Values, 1, vm.NxProp);
      Selector.Height = 20;
      SelectorDockPanel.Children.Add(Selector);

      SelectionChangedEventHandler selectionChanged = (_, args) =>
      {
         if (args.AddedItems.Count > 0 && !args.AddedItems[0]!.Equals(vm.Target))
         {
            Selector.GetBindingExpression(System.Windows.Controls.Primitives.Selector.SelectedItemProperty)
                   ?.UpdateSource();
            ViewModel.RefreshContent();
         }
      };

      Selector.SelectionChanged += selectionChanged;
      Unloaded += (_, _) => Selector.SelectionChanged -= selectionChanged;
   }

   public void RefreshSelector()
   {
      Selector.ItemsSource = ViewModel.Embedded.GetGlobalItemsNonGeneric().Values;
      Selector.GetBindingExpression(System.Windows.Controls.Primitives.Selector.SelectedItemProperty)?.UpdateTarget();
   }

   private void SetEmptyButton_Click(object sender, RoutedEventArgs e)
   {
      var empty = EmptyRegistry.Empties[ViewModel.Target.GetNxPropType(ViewModel.NxProp)];
      Nx.ForceSet(empty, ViewModel.Target, ViewModel.NxProp);

      Selector.SetSelectedItem(empty, string.Empty);

      if (ViewModel.IsExpanded)
         ViewModel.RefreshContent();
   }

   private void ExpandButton_Click(object sender, RoutedEventArgs e)
   {
      ViewModel.IsExpanded = !ViewModel.IsExpanded;
   }
}