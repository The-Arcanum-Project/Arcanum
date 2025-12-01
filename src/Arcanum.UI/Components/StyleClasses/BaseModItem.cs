using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.StyleClasses;

public class BaseModItem : INotifyPropertyChanged
{
   public DataSpace DataSpace
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = DataSpace.Empty;

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
      var dataSpacePath = IO.SelectFolder(IO.GetUserModFolderPath, "Select a base mod folder") ??
                          string.Empty;

      if (string.IsNullOrEmpty(dataSpacePath))
         return;

      DataSpace = CreateBaseModDataSpace(dataSpacePath);
   }

   public static DataSpace CreateBaseModDataSpace(string path)
   {
      return new(Path.GetDirectoryName(path) ?? path, path.Split('/'), DataSpace.AccessType.ReadOnly);
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}