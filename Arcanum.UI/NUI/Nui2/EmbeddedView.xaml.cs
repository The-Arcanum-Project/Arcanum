using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen;
using Nexus.Core;

namespace Arcanum.UI.NUI.Nui2;

public partial class EmbeddedView
{
   public PropertyEditorViewModel ViewModel { get; set; }

   public EmbeddedView(PropertyEditorViewModel vm)
   {
      ViewModel = vm;
      DataContext = ViewModel;
      InitializeComponent();
      var selector = NEF.ObjectSelector(vm.Target, vm.Target.GetGlobalItemsNonGeneric().Values, 1);
      selector.Height = 20;
      SelectorDockPanel.Children.Add(selector);
   }

   private void SetEmptyButton_Click(object sender, RoutedEventArgs e)
   {
      var empty = EmptyRegistry.Empties[ViewModel.Target.GetNxPropType(ViewModel.NxProp)];
      Nx.ForceSet(empty, ViewModel.Target, ViewModel.NxProp);

      if (ViewModel.IsExpanded)
         ViewModel.RefreshContent();
   }

   private void ExpandButton_Click(object sender, RoutedEventArgs e)
   {
      ViewModel.IsExpanded = !ViewModel.IsExpanded;
   }
}