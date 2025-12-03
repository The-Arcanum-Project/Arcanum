using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;
using Arcanum.UI.NUI.Generator;

namespace Arcanum.UI.NUI.Nui2;

public partial class EmbeddedView
{
   public PropertyEditorViewModel ViewModel { get; set; }
   private AutoCompleteComboBox Selector { get; set; }
   private MultiSelectPropertyViewModel _mspvm;

   public EmbeddedView(PropertyEditorViewModel vm, MultiSelectPropertyViewModel mspvm)
   {
      _mspvm = mspvm;
      var binding = new Binding(nameof(mspvm.Value))
      {
         Source = mspvm,
         Mode = BindingMode.TwoWay,
         UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
      };
      ViewModel = vm;
      DataContext = ViewModel;
      InitializeComponent();
      Selector = NEF.ObjectSelector(vm.Target, vm.Embedded.GetGlobalItemsNonGeneric().Values, binding, 1);
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
      _mspvm.Value = empty;

      Selector.SetSelectedItem(empty, string.Empty);

      if (ViewModel.IsExpanded)
         ViewModel.RefreshContent();
   }

   private void ExpandButton_Click(object sender, RoutedEventArgs e)
   {
      ViewModel.IsExpanded = !ViewModel.IsExpanded;
   }
}