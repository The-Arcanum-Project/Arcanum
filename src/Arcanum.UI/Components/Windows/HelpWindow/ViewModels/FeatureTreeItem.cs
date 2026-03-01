using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.UI.AppFeatures;

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public sealed class FeatureTreeItem(IAppFeature feature) : INotifyPropertyChanged
{
   public List<LocationGridCell> LocationGridCells { get; }
   public IAppFeature Feature { get; } = feature;
   public ObservableCollection<FeatureTreeItem> Children { get; } = [];
   public bool IsExpanded
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = true;
   public bool IsVisible
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = true;

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}