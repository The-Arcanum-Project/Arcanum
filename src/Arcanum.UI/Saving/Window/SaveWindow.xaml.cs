using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.UiUtils;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.Saving.Backend;

namespace Arcanum.UI.Saving.Window;

/// <summary>
/// The window has multiple tasks:
/// 1. Assign new objects to files
/// 2. Move objects between files
/// </summary>
/// <remarks>
/// For the first task, the window is in New Object Mode.
/// Only the file and object list are shown.
/// The file list is initially empty, and all new objects are in the object list.
/// Once an object is dragged, all valid files appear to drag them into it.
/// For the second task, the window is in normal mode.
/// The user selects a descriptor, and a file list appears with all files of said descriptor.
/// There a file can be selected, and the objects appear which are in it.
/// Then the object can be dragged to any other file in the descriptor list.
/// It should be possible to only show the changed objects and their corresponding files.
/// Additionally, a file can be moved by selecting it and right-clicking, and a dropDown appears with all valid files.
/// Also, a new file can be created by clicking the "add" button.
/// The new file is limited to the descriptor currently selected or in New Object Mode to the descriptor of the selected object.
/// </remarks>
public partial class SaveWindow
{
   private Point? _dragStartPoint;

   private readonly Queastor _newFileQuaestor;
   private readonly List<IEu5Object> _newObjects;

   private readonly SavingWrapperManager _savingWrapperManager = new ();

   private FileDescriptor? _currentDescriptor;

   // TODO: @Melco: ???
#pragma warning disable CS0414 // Field is assigned but its value is never used
   private bool _showOnlyChangedFiles = true;
#pragma warning restore CS0414 // Field is assigned but its value is never used

   private readonly HashSet<IEu5Object> _changedObjects;

   private readonly HashSet<Eu5FileObj> _relevantFiles = [];

   private List<FileDescriptor> _descriptorsWithChangedFiles = [];

   #region UI Bindings

   public static readonly DependencyProperty SearchResultProperty = DependencyProperty.Register(nameof(SearchResult),
                                                                                                typeof(ObservableCollection<ISearchable>),
                                                                                                typeof(SaveWindow),
                                                                                                new (default(ObservableCollection<ISearchable>)));

   public ObservableCollection<ISearchable> SearchResult
   {
      get => (ObservableCollection<ISearchable>)GetValue(SearchResultProperty);
      set => SetValue(SearchResultProperty, value);
   }

   public static readonly DependencyProperty ShownFilesProperty = DependencyProperty.Register(nameof(ShownFiles),
                                                                                              typeof(SortedObservableCollection<Eu5FileObj>),
                                                                                              typeof(SaveWindow),
                                                                                              new (new SortedObservableCollection<Eu5FileObj>([])));

   public SortedObservableCollection<Eu5FileObj> ShownFiles
   {
      get => (SortedObservableCollection<Eu5FileObj>)GetValue(ShownFilesProperty);
      set => SetValue(ShownFilesProperty, value);
   }

   public static readonly DependencyProperty ShownDescriptorsProperty =
      DependencyProperty.Register(nameof(ShownDescriptors),
                                  typeof(ObservableCollection<FileDescriptor>),
                                  typeof(SaveWindow),
                                  new (new ObservableCollection<FileDescriptor>([])));

   public ObservableCollection<FileDescriptor> ShownDescriptors
   {
      get => (ObservableCollection<FileDescriptor>)GetValue(ShownDescriptorsProperty);
      set => SetValue(ShownDescriptorsProperty, value);
   }

   public static readonly DependencyProperty ShownObjectsProperty = DependencyProperty.Register(nameof(ShownObjects),
                                                                                                typeof(ObservableCollection<IEu5Object>),
                                                                                                typeof(SaveWindow),
                                                                                                new (new ObservableCollection<IEu5Object>([])));

   public ObservableCollection<IEu5Object> ShownObjects
   {
      get => (ObservableCollection<IEu5Object>)GetValue(ShownObjectsProperty);
      set => SetValue(ShownObjectsProperty, value);
   }

   public static readonly DependencyProperty NewFileModeProperty =
      DependencyProperty.Register(nameof(NewFileMode), typeof(bool), typeof(SaveWindow), new (false));

   public bool NewFileMode
   {
      get => (bool)GetValue(NewFileModeProperty);
      set => SetValue(NewFileModeProperty, value);
   }

   public static readonly DependencyProperty ValidFileProperty =
      DependencyProperty.Register(nameof(ValidFiles),
                                  typeof(List<Eu5FileObj>),
                                  typeof(SaveWindow),
                                  new (default(List<Eu5FileObj>)));

   public List<Eu5FileObj> ValidFiles
   {
      get => (List<Eu5FileObj>)GetValue(ValidFileProperty);
      set => SetValue(ValidFileProperty, value);
   }

   #endregion

   public SaveWindow()
   {
      var newObjectsList = SaveMaster.GetNewSaveables();

      var newObjects = newObjectsList.SelectMany(kv => kv.Value).ToList();

      _newObjects = newObjects;
      _changedObjects = SaveMaster.GetAllModifiedObjects().ToHashSet();

      foreach (var file in _changedObjects.Select(changedObject => changedObject.Source))
      {
         _relevantFiles.Add(file);
         _descriptorsWithChangedFiles.Add(file.Descriptor); //TODO we crashed here once no Idea why?
      }

      _descriptorsWithChangedFiles = _descriptorsWithChangedFiles.Distinct().ToList();
      _descriptorsWithChangedFiles.Sort(new FileDescriptorComparer());

      _newFileQuaestor = new (new ());
      foreach (var iEu5Object in newObjects)
         _newFileQuaestor.AddToIndex(iEu5Object);
      _newFileQuaestor.RebuildBkTree();
      InitializeComponent();
      SetupUi();
      if (newObjects.Count > 0)
      {
         NewObjectModeToggle.IsChecked = true;
         SetUpNewObjectMode();
      }
      else
      {
         NewObjectModeToggle.IsChecked = false;
         SetUpNormalMode();
      }
   }

   private void SetupUi()
   {
      SearchBox.RequestSearch = SearchBoxRequestSearch;
      SearchBox.SettingsOpened = OpenSettingsWindow;
      SearchBoxPopout.RequestSearch = SearchBoxPopoutRequestSearch;
      SearchBoxPopout.SearchInputTextBox.PreviewKeyDown += (_, e) =>
      {
         if (e.Key == Key.Escape)
         {
            MoveObjectPopup.IsOpen = false;
            ObjectListView.Focus();
         }

         if (e.Key != Key.Down || MovableFilesListView.Items.Count <= 0)
            return;

         MovableFilesListView.SelectedIndex = 0;
         var listViewItem = (ListViewItem)MovableFilesListView.ItemContainerGenerator.ContainerFromIndex(0);
         listViewItem?.Focus();
         e.Handled = true;
      };
      SearchBox.SearchInputTextBox.PreviewKeyDown += (_, e) =>
      {
         if (e.Key != Key.Down || ResultView.Items.Count <= 0)
            return;

         ResultView.SelectedIndex = 0;
         var listViewItem = (ListViewItem)ResultView.ItemContainerGenerator.ContainerFromIndex(0);
         listViewItem?.Focus();
         e.Handled = true;
      };
   }

   private void SetUpNewObjectMode()
   {
      NewFileMode = true;
      DescriptorsColumn.MinWidth = 0;
      DescriptorsColumn.Width = new (0);
      SplitterColumn.Width = new (0);
      ShownFiles.Clear();
      ShownObjects = new (_newObjects);
   }

   private void SetUpNormalMode()
   {
      NewFileMode = false;
      DescriptorsColumn.MinWidth = 100;
      DescriptorsColumn.Width = new (1, GridUnitType.Star);
      SplitterColumn.Width = GridLength.Auto;
      _currentDescriptor = null;
      ShownFiles.Clear();
      ShownObjects.Clear();
      ShownDescriptors = new (_descriptorsWithChangedFiles);
   }

   private void SearchBoxRequestSearch(string obj)
   {
      if (NewFileMode)
         SearchResult = new (_newFileQuaestor.Search(obj));
   }

   private void OpenSettingsWindow()
   {
      throw new NotImplementedException();
   }

   #region UI Events

   private void OnDropInFile(object sender, DragEventArgs e)
   {
      if (_currentDescriptor is null)
         return;

      var file = _savingWrapperManager.GetFile(GetFileFromItem(sender));
      if (NewFileMode)
      {
         TransferObjectsTo(file);
      }
      else
      {
         var sourceFile = FileListView.SelectedItem as Eu5FileObj ?? throw new InvalidOperationException();
         var sourceWrapper = _savingWrapperManager.GetFile(sourceFile);
         TransferObjectsTo(sourceWrapper, file);
      }
   }

   private void TransferObjectsTo(FileSavingWrapper file)
   {
      for (var index = ObjectListView.SelectedItems.Count - 1; index >= 0; index--)
      {
         var obj = ObjectListView.SelectedItems[index] as IEu5Object ?? throw new InvalidOperationException();
         TransferObjectTo(obj, file);
      }
   }

   private void TransferObjectsTo(FileSavingWrapper source, FileSavingWrapper dest)
   {
      for (var index = ObjectListView.SelectedItems.Count - 1; index >= 0; index--)
      {
         var obj = ObjectListView.SelectedItems[index] as IEu5Object ?? throw new InvalidOperationException();
         TransferObjectTo(obj, source, dest);
      }
   }

   private void OnDragInFile(object sender, DragEventArgs e)
   {
      e.Effects = DragDropEffects.Move;
      if (_currentDescriptor is null)
         e.Effects = DragDropEffects.None;
      else if (!NewFileMode)
      {
         // Check if the file is the same:
         var file = GetFileFromItem(sender);
         if (Equals(file, FileListView.SelectedItem))
            e.Effects = DragDropEffects.None;
      }

      e.Handled = true;
   }

   //TODO @MelCo: Convert to Behavior
   private void ObjectLeftButtonPreview(object sender, MouseButtonEventArgs e)
   {
      _dragStartPoint = e.GetPosition(null);
   }

   private void ObjectLeftButtonMove(object sender, MouseEventArgs e)
   {
      if (!_dragStartPoint.HasValue || Mouse.LeftButton != MouseButtonState.Pressed)
         return;

      var currentPosition = e.GetPosition(null);
      var distance = (currentPosition - _dragStartPoint.Value).Length;

      if (distance < Config.START_DRAG_DISTANCE)
         return;

      var data = new DataObject();
      data.SetData("Test", "a");
      _dragStartPoint = null;
      try
      {
         DragDrop.DoDragDrop(ObjectListView, data, DragDropEffects.Move);
      }
      catch (Exception error) //finally
      {
         //Console.WriteLine("Error: " + error.Message);
         //ShownFiles.Clear();
      }
   }

   private void NewObjectModeToggle_OnChecked(object sender, RoutedEventArgs e)
   {
      SetUpNewObjectMode();
   }

   private void NewObjectModeToggle_OnUnchecked(object sender, RoutedEventArgs e)
   {
      SetUpNormalMode();
   }

   private void ObjectListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (!NewFileMode)
         return;

      if (ObjectListView.SelectedItems.Count < 1)
      {
         ShownFiles.Clear();
         _currentDescriptor = null;
         return;
      }

      var selectedDescriptor =
         GetDescriptorFromItem(ObjectListView.SelectedItems[0] as IEu5Object ?? throw new InvalidOperationException());
      for (var index = 1; index < ObjectListView.SelectedItems.Count; index++)
      {
         var item = ObjectListView.SelectedItems[index];
         var descriptor = GetDescriptorFromItem(item as IEu5Object ?? throw new InvalidOperationException());
         if (Equals(descriptor, selectedDescriptor))
            continue;

         ShownFiles.Clear();
         _currentDescriptor = null;
         return;
      }

      _currentDescriptor = selectedDescriptor;
      ShownFiles = new (_savingWrapperManager.GetAllFiles(selectedDescriptor));
      // Check if all new objects have a common descriptor
   }

   private void FileListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (NewFileMode)
         return;

      if (FileListView.SelectedItems.Count < 1)
      {
         ShownObjects.Clear();
         return;
      }

      var file = FileListView.SelectedItem as Eu5FileObj ?? throw new InvalidOperationException();
      ShownObjects = new (_savingWrapperManager.GetAllRelevantObjects(file, _changedObjects));
   }

   private void DescriptionListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (NewFileMode)
         return;

      if (DescriptionListView.SelectedItems.Count < 1)
      {
         ShownFiles.Clear();
         _currentDescriptor = null;
         return;
      }

      _currentDescriptor =
         DescriptionListView.SelectedItem as FileDescriptor ?? throw new InvalidOperationException();
      ShownFiles = new (_savingWrapperManager.GetAllRelevantFiles(_currentDescriptor, _relevantFiles));
   }

   #endregion

   #region Helper

   private void TransferObjectTo(IEu5Object obj, FileSavingWrapper targetFile)
   {
      if (!NewFileMode)
         return;

      targetFile.AddObject(obj);
      _newObjects.Remove(obj);
      _changedObjects.Add(obj);
      _newFileQuaestor.RemoveFromIndex(obj);
      ShownObjects.Remove(obj);
   }

   private void TransferObjectTo(IEu5Object obj, FileSavingWrapper source, FileSavingWrapper targetFile)
   {
      //TODO @MelCo: Update FileList in normal mode
      if (NewFileMode)
         return;

      source.TransferObjectTo(obj, targetFile);
      _changedObjects.Add(obj);
      ShownObjects.Remove(obj);
      // Binary search files to find new input position if already there do not add
      ShownFiles.TryInsertSorted(targetFile.FileObj, new Eu5FileComparer());
   }

   private Eu5FileObj GetFileFromItem(object sender)
   {
      if (FileListView.ContainerFromElement((DependencyObject)sender) is ListViewItem container)
         return (Eu5FileObj)container.Content; // Source object

      throw new ArgumentException("Sender is not a ListViewItem");
   }

   private FileDescriptor GetDescriptorFromItem(IEu5Object obj)
   {
      return obj.Source.Descriptor;
   }

   #endregion

   private void AddNewFile(object sender, RoutedEventArgs e)
   {
      if (_currentDescriptor is null)
      {
         MBox.Show("Please select a object first to create a new file for it.",
                   "No object selected",
                   icon: MessageBoxImage.Warning,
                   owner: this);
         return;
      }

      var dialog = new CreateNewFile(_currentDescriptor, _savingWrapperManager) { Owner = this };
      dialog.ShowDialog();
      if (dialog.DialogResult != true)
         return;

      var newFile = _savingWrapperManager.GetFile(new (dialog.NewPath, _currentDescriptor));
      var descriptor = _savingWrapperManager.GetDescriptor(_currentDescriptor);
      descriptor.AddNewFile(newFile);
      ShownFiles = new (descriptor.AllFiles);
   }

   private void SelectSearchResult(object sender, MouseButtonEventArgs e)
   {
      if (ResultView.ContainerFromElement((DependencyObject)sender) is not ListViewItem container)
         return;

      HighlightSearchResult((ISearchable)container.Content);
   }

   private void SelectSearchResultKey(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Up && ResultView.SelectedIndex == 0)
      {
         SearchBox.SearchInputTextBox.Focus();
         ResultView.SelectedIndex = -1;
         e.Handled = true;
         return;
      }

      if (e.Key != Key.Enter)
         return;

      if (ResultView.SelectedItem is ISearchable selected)
         HighlightSearchResult(selected);
   }

   private void HighlightSearchResult(ISearchable searchable)
   {
      if (searchable is IEu5Object)
      {
         var firstOrDefault = ShownObjects.FirstOrDefault(x => x!.Equals(searchable), null);
         if (firstOrDefault is null)
            return;

         ObjectListView.SelectedItem = firstOrDefault;
      }

      SearchResult.Clear();
   }

   private void ObjectRightButtonPreview(object sender, MouseButtonEventArgs e)
   {
      if (_currentDescriptor == null)
         return;
      if (sender is not UIElement element ||
          TreeTraversal.FindVisualParent<ListViewItem>(element) is not { } selectedItem)
         return;

      e.Handled = true;
      if (!ObjectListView.SelectedItems.Contains(selectedItem.Content))
         return;

      OpenFileTransferPopup();
   }

   private void SelectFileTransferKey(object sender, KeyEventArgs e)
   {
      if (e.Key == Key.Up && MovableFilesListView.SelectedIndex == 0)
      {
         SearchBox.SearchInputTextBox.Focus();
         MovableFilesListView.SelectedIndex = -1;
         e.Handled = true;
         return;
      }

      if (e.Key != Key.Enter || _currentDescriptor == null)
         return;

      if (Keyboard.FocusedElement is not ListViewItem selectedItem)
         return;

      OpenFileTransferPopup(selectedItem);
   }

   private void OpenFileTransferPopup(ListViewItem selectedItem)
   {
      MoveObjectPopup.PlacementTarget = selectedItem;
      MoveObjectPopup.Placement = PlacementMode.Bottom;
      MoveObjectPopup.HorizontalOffset = 0;
      MoveObjectPopup.VerticalOffset = 0;
      MoveObjectPopup.IsOpen = true;
      SearchBoxPopout.SearchInputTextBox.Focus();
      SearchBoxPopoutRequestSearch("");
   }

   private void OpenFileTransferPopup()
   {
      MoveObjectPopup.Placement = PlacementMode.MousePoint;
      MoveObjectPopup.HorizontalOffset = -10;
      MoveObjectPopup.VerticalOffset = -10;
      MoveObjectPopup.IsOpen = true;
      SearchBoxPopout.SearchInputTextBox.Focus();
      SearchBoxPopoutRequestSearch("");
   }

   private void SearchBoxPopoutRequestSearch(string obj)
   {
      if (_currentDescriptor == null)
         return;

      if (string.IsNullOrEmpty(obj))
         ValidFiles = _savingWrapperManager.GetAllFiles(_currentDescriptor);
   }

   private void TransferToFileSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (MovableFilesListView.SelectedItem is not Eu5FileObj file)
         return;
      if (FileListView.SelectedItem is not Eu5FileObj sourceFile)
         return;

      TransferObjectsTo(_savingWrapperManager.GetFile(sourceFile), _savingWrapperManager.GetFile(file));

      MoveObjectPopup.IsOpen = false;
      ObjectListView.Focus();
   }

   private void SaveWindow_OnClosed(object? sender, EventArgs e)
   {
      var changes = _savingWrapperManager.GetAllChangedFiles();
      foreach (var change in changes)
      {
         /*
         Console.WriteLine("File: " + change.FileObj);
         Console.WriteLine("| Added: ");
         foreach (var addedObject in change.AddedObjects)
            Console.WriteLine("|  " + addedObject.ResultName);

         Console.WriteLine("| Transferred: ");
         foreach (var transferredObject in change.TransferredObjects)
            Console.WriteLine("|  " + transferredObject.ResultName);

         Console.WriteLine("-------------------");*/
      }
   }
}