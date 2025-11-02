using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ParsingViewer : INotifyPropertyChanged
{
   private FileDescriptor? _currentDescriptor;
   public FileDescriptor? CurrentDescriptor
   {
      get => _currentDescriptor;
      set
      {
         if (Equals(value, _currentDescriptor))
            return;

         _currentDescriptor = value;
         OnPropertyChanged();
      }
   }

   public ParsingViewer()
   {
      InitializeComponent();
   }

   private void ParsingListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (ParsingListView.SelectedItem is not FileDescriptor descriptor)
         return;

      CurrentDescriptor = descriptor;
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }
}