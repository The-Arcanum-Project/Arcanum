using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.IO;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.StyleClasses;

public class BaseModItem : INotifyPropertyChanged
{
   private string _path = string.Empty;
   public string Path
   {
      get => _path;
      set
      {
         if (_path == value)
            return;

         _path = value;
         OnPropertyChanged();
      }
   }

   public ICommand SelectFolderCommand { get; }
   public ICommand RemoveCommand { get; }

   public BaseModItem(Action<BaseModItem> removeCallback)
   {
      SelectFolderCommand = new RelayCommand(OpenFolderDialog);
      RemoveCommand =
         new RelayCommand(() => { Application.Current.Dispatcher.BeginInvoke(() => removeCallback(this)); });
   }

   private void OpenFolderDialog()
   {
      Path = IO.SelectFolder(IO.GetUserModFolderPath, "Select a base mod folder") ??
             string.Empty;
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}