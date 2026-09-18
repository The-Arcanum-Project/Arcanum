#region

using System.Windows;
using Arcanum.UI.Components.Helpers;

#endregion

namespace Arcanum.UI.Components.Windows.MinorWindows.LocationColorPicker;

public sealed partial class LocationColorPicker
{
   public LocationColorPicker()
   {
      InitializeComponent();
      ViewModel = new();
      DataContext = ViewModel;
   }

   public LocationColorPickerViewModel ViewModel { get; }

   private void OpenReferencePicker_Click(object sender, RoutedEventArgs e)
   {
      var result = PopupService.ShowColorPicker(ViewModel.ReferenceColor, PointToScreen(new(0, 0)));
      if (result.Confirmed)
         ViewModel.ReferenceColor = result.Value;
   }

   private void OpenResultPicker_Click(object sender, RoutedEventArgs e)
   {
      var result = PopupService.ShowColorPicker(ViewModel.SelectedColor, PointToScreen(new(0, 0)));
      if (result.Confirmed)
         ViewModel.SelectedColor = result.Value;
   }
}