#region

using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.InGame.Cultural;
using Arcanum.Core.GameObjects.InGame.Pops;
using Arcanum.Core.GameObjects.InGame.Religious;

#endregion

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public sealed partial class MassPopPainterView
{
   private MassPopPainterViewModel? ViewModel { get; set; }
   public MassPopPainterView()
   {
      InitializeComponent();

      Loaded += (s, e) =>
      {
         SyncAllControls();
      };

      DataContextChanged += (_, e) =>
      {
         if (e.NewValue is MassPopPainterViewModel newVm)
         {
            ViewModel?.UIResetRequested -= SyncAllControls;
            ViewModel = newVm;
            ViewModel.UIResetRequested += SyncAllControls;
         }
      };
   }

   private void SyncAllControls()
   {
      if (ViewModel == null)
         return;

      if (ViewModel.SourceCulture != Culture.Empty)
         CultureFilterBox.SetSelectedItem(ViewModel.SourceCulture, ViewModel.SourceCulture.ToString());
      if (ViewModel.TargetCulture != Culture.Empty)
         CultureTargetBox.SetSelectedItem(ViewModel.TargetCulture, ViewModel.TargetCulture.ToString());

      if (ViewModel.SourceReligion != Religion.Empty)
         ReligionFilterBox.SetSelectedItem(ViewModel.SourceReligion, ViewModel.SourceReligion.ToString());
      if (ViewModel.TargetReligion != Religion.Empty)
         ReligionTargetBox.SetSelectedItem(ViewModel.TargetReligion, ViewModel.TargetReligion.ToString());

      if (ViewModel.SourcePopType != PopType.Empty)
         PopTypeFilterBox.SetSelectedItem(ViewModel.SourcePopType, ViewModel.SourcePopType.ToString());
      if (ViewModel.TargetPopType != PopType.Empty)
         PopTypeTargetBox.SetSelectedItem(ViewModel.TargetPopType, ViewModel.TargetPopType.ToString());
   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.OnRequestRefreshPreview();
   }

   private void ClearCultureTarget_OnClick(object sender, RoutedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.TargetCulture = Culture.Empty;

      CultureTargetBox.SetSelectedItem(Culture.Empty, string.Empty);
   }

   private void ClearReligionTarget_OnClick(object sender, RoutedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.TargetReligion = Religion.Empty;
      ReligionTargetBox.SetSelectedItem(Religion.Empty, string.Empty);
   }

   private void ClearPopTypeTarget_OnClick(object sender, RoutedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.TargetPopType = PopType.Empty;
      PopTypeTargetBox.SetSelectedItem(PopType.Empty, string.Empty);
   }

   public void ClearCultureFilter_OnClick(object sender, RoutedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.SourceCulture = Culture.Empty;
      CultureFilterBox.SetSelectedItem(Culture.Empty, string.Empty);
   }

   public void ClearReligionFilter_OnClick(object sender, RoutedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.SourceReligion = Religion.Empty;
      ReligionFilterBox.SetSelectedItem(Religion.Empty, string.Empty);
   }

   public void ClearPopTypeFilter_OnClick(object sender, RoutedEventArgs e)
   {
      if (DataContext is not MassPopPainterViewModel vm)
         return;

      vm.SourcePopType = PopType.Empty;
      PopTypeFilterBox.SetSelectedItem(PopType.Empty, string.Empty);
   }
}