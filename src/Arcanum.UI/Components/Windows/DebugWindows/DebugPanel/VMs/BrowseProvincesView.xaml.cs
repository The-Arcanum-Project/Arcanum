using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.UI.Components.Windows.DebugWindows.DebugPanel.VMs;

public partial class BrowseProvincesView
{
   public ICollectionView ItemsView { get; }

   public string FilterText
   {
      get;
      set
      {
         field = value;
         ItemsView.Refresh();
      }
   } = string.Empty;

   public BrowseProvincesView()
   {
      InitializeComponent();

      ItemsView = CollectionViewSource.GetDefaultView(Globals.Locations.Values);
      ItemsView.Filter = FilterItem;
   }

   private bool FilterItem(object obj)
   {
      if (string.IsNullOrWhiteSpace(FilterText))
         return true;

      if (obj is Location s)
         return s.UniqueId.Contains(FilterText, StringComparison.OrdinalIgnoreCase);

      return false;
   }

   private void FilterProvincesTextBox_TextChanged(object sender, TextChangedEventArgs e)
   {
      FilterText = FilterProvincesTextBox.Text;
   }

   private void ProvincesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      ProvincePropertyGrid.SelectedObject = ProvincesListBox.SelectedItem;
   }
}