using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.UI.Components.Windows.MainWindows;
using Common;
using Common.UI;

namespace Arcanum.UI.Components.Windows.PopUp;

public class FileChangeInfo(WatcherChangeTypes action, string filePath) : DependencyObject, INotifyPropertyChanged
{
   private WatcherChangeTypes _typeAction = action;
   private string _filePath = filePath;

   public WatcherChangeTypes TypeAction
   {
      get => _typeAction;
      set
      {
         _typeAction = value;
         OnPropertyChanged(nameof(TypeAction));
         OnPropertyChanged(nameof(Action));
         OnPropertyChanged(nameof(ActionColor));
      }
   }

   private string GetActionFromType()
   {
      string action;
      // check the flags in _typeAction and set Action accordingly, multiple flags may be set.
      // if created and renamed are both set, prioritize created.
      // if deleted and renamed are both set, prioritize deleted.
      // if changed and renamed are both set, prioritize changed.
      // if changed and created are both set, prioritize created.
      // if deleted and created are both set, change to "changed".

      var isCreated = _typeAction.HasFlag(WatcherChangeTypes.Created);
      var isDeleted = _typeAction.HasFlag(WatcherChangeTypes.Deleted);
      var isChanged = _typeAction.HasFlag(WatcherChangeTypes.Changed);
      var isRenamed = _typeAction.HasFlag(WatcherChangeTypes.Renamed);

      // The logic is structured to handle the highest priority cases first.
      if (isCreated && isDeleted)
         // Special case: An item created and then deleted within the same operation
         // is effectively a change from the initial state, resulting in no new file.
         action = "C";
      else if (isCreated)
         // 'Created' takes precedence over 'Changed' and 'Renamed'.
         action = "A";
      else if (isDeleted)
         // 'Deleted' takes precedence over 'Renamed'.
         action = "D";
      else if (isChanged)
         // 'Changed' takes precedence over 'Renamed'.
         action = "C";
      else if (isRenamed)
         action = "R";
      else
         action = "U"; // Unknown or no change detected.

      return action;
   }

   public string Action => GetActionFromType();

   public string FilePath
   {
      get => _filePath;
      set
      {
         if (value == _filePath)
            return;

         _filePath = value;
         OnPropertyChanged(nameof(FilePath));
      }
   }

   public SolidColorBrush ActionColor
   {
      get
      {
         switch (Action)
         {
            case "A":
               return Brushes.Green;
            case "D":
               return Brushes.Red;
            case "C":
               return Brushes.Blue;
            case "R":
               return Brushes.Purple;
            default:
               return Brushes.Black;
         }
      }
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected void OnPropertyChanged(string propertyName)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}

public partial class FileChange
{
   public FileChangedEventArgs Args = null!;

   public static readonly DependencyProperty FileChangesProperty = DependencyProperty.Register(nameof(FileChanges),
       typeof(ObservableCollection<FileChangeInfo>),
       typeof(FileChangeInfo),
       new(default(ObservableCollection<FileChangeInfo>)));

   public ObservableCollection<FileChangeInfo> FileChanges
   {
      get => (ObservableCollection<FileChangeInfo>)GetValue(FileChangesProperty);
      set => SetValue(FileChangesProperty, value);
   }

   public static readonly DependencyProperty InfoTextProperty =
      DependencyProperty.Register(nameof(InfoText),
                                  typeof(string),
                                  typeof(FileChange),
                                  new("Rename detected in tracked files. Verify changes below:"));

   public string InfoText
   {
      get { return (string)GetValue(InfoTextProperty); }
      set { SetValue(InfoTextProperty, value); }
   }

   public bool ShowReloadSaveButtons
   {
      get => (bool)GetValue(ShowReloadSaveButtonsProperty);
      set => SetValue(ShowReloadSaveButtonsProperty, value);
   }

   public bool ShowOkButton
   {
      get => (bool)GetValue(ShowOkButtonProperty);
      set => SetValue(ShowOkButtonProperty, value);
   }

   public static FileChange Instance { get; private set; } = new();

   public bool IsShown;

   public static readonly DependencyProperty ShowOkButtonProperty =
      DependencyProperty.Register(nameof(ShowOkButton), typeof(bool), typeof(FileChange), new(true));

   public static readonly DependencyProperty ShowReloadSaveButtonsProperty =
      DependencyProperty.Register(nameof(ShowReloadSaveButtons),
                                  typeof(bool),
                                  typeof(FileChange),
                                  new(false));

   public FileChange()
   {
      InitializeComponent();
      FileChanges = [];
      Unloaded += (_, _) =>
      {
         lock (Instance)
            Instance = new();
      };
   }

   private void ChangeToPanicMode()
   {
      InfoText =
         "Changes detected in tracked files! Currently Arcanum is not able to automatically synchronize with the file system. Please choose one of the options below to proceed:";
      ShowOkButton = false;
      ShowReloadSaveButtons = true;
   }

   public static void Show(FileChangedEventArgs args)
   {
      Application.Current.Dispatcher.Invoke(() =>
      {
         Instance.AddFileChangeEvent(args.ChangeType, args.FullPath, args.OldFullPath);
         lock (Instance)
            if (Instance.IsShown)
               return;

         Instance.Args = args;
         Instance.Show();
         Instance.IsShown = true;
      });
   }

   public void AddFileChangeEvent(WatcherChangeTypes action, string filePath, string? oldFilePath = null)
   {
      var existingChange = FileChanges.FirstOrDefault(f => f.FilePath == (oldFilePath ?? filePath));

      if (existingChange != null)
      {
         if (action.HasFlag(WatcherChangeTypes.Renamed) && oldFilePath != null)
            existingChange.FilePath = filePath;

         // remove the old entry if the file was delteded and created before
         if (existingChange.TypeAction.HasFlag(WatcherChangeTypes.Created) &&
             action.HasFlag(WatcherChangeTypes.Deleted) &&
             !existingChange.TypeAction.HasFlag(WatcherChangeTypes.Deleted))
            FileChanges.Remove(existingChange);

         existingChange.TypeAction |= action;
      }
      else
      {
         FileChanges.Add(new(action, filePath));
      }

      if (FileChanges.Any(f => f.Action != "R"))
         ChangeToPanicMode();
   }

   private void ButtonBase_OnClickOk(object sender, RoutedEventArgs e)
   {
      Close();
   }

   private void ButtonBase_OnClickSave(object sender, RoutedEventArgs e)
   {
      SaveMaster.SaveAll();
      FileStateManager.ReloadFile(Args);
      Close();
   }

   private void ButtonBase_OnClickDiscard(object sender, RoutedEventArgs e)
   {
      FileStateManager.ReloadFile(Args);
      Close();
   }

   private void ButtonBase_OnClickMenu(object sender, RoutedEventArgs e)
   {
      UIHandle.Instance.MainWindowsHandle.TransferToMainMenuScreen(this, MainMenuScreen.MainMenuScreenView.Arcanum);
   }
}