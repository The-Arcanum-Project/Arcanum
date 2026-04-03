#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public sealed class FeatureItem(FeatureDoc documentation) : INotifyPropertyChanged
{
   public FeatureDoc Documentation { get; } = documentation;
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