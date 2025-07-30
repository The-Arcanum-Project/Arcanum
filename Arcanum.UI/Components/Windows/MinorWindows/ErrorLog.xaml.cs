using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.ErrorSystem;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ErrorLog
{
   public enum FilterType
   {
      Severity,
      Name,
      Id,
      Message,
      Description,
      ErrorAction,
      Resolution,
   }
   
   public ErrorLog()
   {
      InitializeComponent();
      
      FilterComboBox.ItemsSource = Enum.GetValues(typeof(FilterType));
      ErrorLogListView.ItemsSource = new ListCollectionView(ErrorManager.Diagnostics);
   }

   private void ErrorLogListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      // Load to the split view
      
   }
}