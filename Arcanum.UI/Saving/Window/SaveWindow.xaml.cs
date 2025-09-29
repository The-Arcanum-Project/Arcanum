using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.API.UtilServices.Search;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
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

    private readonly SavingWrapperManager _savingWrapperManager = new();

    private FileDescriptor? _currentDescriptor;

    private bool _showOnlyChangedFiles = true;

    private List<IEu5Object> _changedObjects;
    
    private List<FileDescriptor> _descriptorsWithChangedFiles;
    
    #region UI Bindings

    public static readonly DependencyProperty SearchResultProperty = DependencyProperty.Register(
        nameof(SearchResult), typeof(ObservableCollection<ISearchable>), typeof(SaveWindow), new PropertyMetadata(default(ObservableCollection<ISearchable>)));

    public ObservableCollection<ISearchable> SearchResult
    {
        get => (ObservableCollection<ISearchable>)GetValue(SearchResultProperty);
        set => SetValue(SearchResultProperty, value);
    }

    public static readonly DependencyProperty ShownFilesProperty = DependencyProperty.Register(
        nameof(ShownFiles), typeof(ObservableCollection<Eu5FileObj>), typeof(SaveWindow),
        new(new ObservableCollection<Eu5FileObj>([])));

    public ObservableCollection<Eu5FileObj> ShownFiles
    {
        get => (ObservableCollection<Eu5FileObj>)GetValue(ShownFilesProperty);
        set => SetValue(ShownFilesProperty, value);
    }

    public static readonly DependencyProperty ShownDescriptorsProperty = DependencyProperty.Register(
        nameof(ShownDescriptors), typeof(ObservableCollection<FileDescriptor>), typeof(SaveWindow),
        new(new ObservableCollection<FileDescriptor>([])));

    public ObservableCollection<FileDescriptor> ShownDescriptors
    {
        get => (ObservableCollection<FileDescriptor>)GetValue(ShownDescriptorsProperty);
        set => SetValue(ShownDescriptorsProperty, value);
    }

    public static readonly DependencyProperty ShownObjectsProperty = DependencyProperty.Register(
        nameof(ShownObjects), typeof(ObservableCollection<IEu5Object>), typeof(SaveWindow),
        new(new ObservableCollection<IEu5Object>([])));

    public ObservableCollection<IEu5Object> ShownObjects
    {
        get => (ObservableCollection<IEu5Object>)GetValue(ShownObjectsProperty);
        set => SetValue(ShownObjectsProperty, value);
    }

    public static readonly DependencyProperty NewFileModeProperty = DependencyProperty.Register(
        nameof(NewFileMode), typeof(bool), typeof(SaveWindow), new PropertyMetadata(false));

    public bool NewFileMode
    {
        get => (bool)GetValue(NewFileModeProperty);
        set => SetValue(NewFileModeProperty, value);
    }

    #endregion

    public SaveWindow(List<IEu5Object> newObjects, List<IEu5Object> changedObjects, bool newMode = false)
    {
        _newObjects = newObjects;
        _changedObjects = changedObjects;
        _descriptorsWithChangedFiles = _changedObjects
            .Select(x => x.Source.Descriptor)
            .Distinct()
            .ToList();
        _descriptorsWithChangedFiles.Sort(new FileDescriptorComparer());
        
        
        _newFileQuaestor = new(new());
        foreach (var iEu5Object in newObjects)
            _newFileQuaestor.AddToIndex(iEu5Object);
        _newFileQuaestor.RebuildBkTree();
        InitializeComponent();
        SetupUi();
        if (newMode)
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
        SearchBox.SearchInputTextBox.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Down || ResultView.Items.Count <= 0) return;
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
        DescriptorsColumn.Width = new GridLength(0);
        SplitterColumn.Width = new GridLength(0);
        ShownFiles.Clear();
        ShownObjects = new(_newObjects);
    }

    private void SetUpNormalMode()
    {
        NewFileMode = false;
        DescriptorsColumn.MinWidth = 100;
        DescriptorsColumn.Width = new GridLength(1, GridUnitType.Star);
        SplitterColumn.Width = GridLength.Auto;
        _currentDescriptor = null;
        ShownFiles.Clear();
        ShownObjects.Clear();
        ShownDescriptors = new(_descriptorsWithChangedFiles);
    }

    private void SearchBoxRequestSearch(string obj)
    {
        if (NewFileMode)
        {
            SearchResult = new(_newFileQuaestor.Search(obj));
        }
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
            for (var index = ObjectListView.SelectedItems.Count - 1; index >= 0; index--)
            {
                var obj = ObjectListView.SelectedItems[index] as IEu5Object ?? throw new InvalidOperationException();
                TransferObjectTo(obj, file);
            }
        }
    }

    private void OnDragInFile(object sender, DragEventArgs e)
    {
        if (_currentDescriptor is null)
            e.Effects = DragDropEffects.None;
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    //TODO @MelCo: Convert to Behavior
    private void ObjectLeftButtonPreview(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
    }

    private void ObjectLeftButtonMove(object sender, MouseEventArgs e)
    {
        if (!_dragStartPoint.HasValue || Mouse.LeftButton != MouseButtonState.Pressed) return;
        var currentPosition = e.GetPosition(null);
        var distance = (currentPosition - _dragStartPoint.Value).Length;

        if (distance < Config.START_DRAG_DISTANCE) return;
        var data = new DataObject();
        data.SetData("Test", "a");
        _dragStartPoint = null;
        try
        {
            DragDrop.DoDragDrop(ObjectListView, data, DragDropEffects.Move);
        }
        catch (Exception error) //finally
        {
            Console.WriteLine("Error: " + error.Message);
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
        if (!NewFileMode) return;
        // Check if all new objects have a common descriptor
        if (ObjectListView.SelectedItems.Count < 1)
        {
            ShownFiles.Clear();
            _currentDescriptor = null;
            return;
        }

        var selectedDescriptor =
            GetDescriptorFromItem(
                ObjectListView.SelectedItems[0] as IEu5Object ?? throw new InvalidOperationException());
        for (var index = 1; index < ObjectListView.SelectedItems.Count; index++)
        {
            var item = ObjectListView.SelectedItems[index];
            var descriptor = GetDescriptorFromItem(item as IEu5Object ?? throw new InvalidOperationException());
            if (Equals(descriptor, selectedDescriptor)) continue;
            ShownFiles.Clear();
            _currentDescriptor = null;
            return;
        }

        _currentDescriptor = selectedDescriptor;
        ShownFiles = new(_savingWrapperManager.GetAllFiles(selectedDescriptor));
    }
    
    private void FileListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(NewFileMode) return;
        
    }

    private void DescriptionListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(NewFileMode) return;
        if (DescriptionListView.SelectedItems.Count < 1)
        {
            ShownFiles.Clear();
            _currentDescriptor = null;
            return;
        }
        _currentDescriptor = DescriptionListView.SelectedItem as FileDescriptor ?? throw new InvalidOperationException();
        ShownFiles = new(_savingWrapperManager.GetAllFiles(_currentDescriptor));
    }

    #endregion

    #region Helper

    private void TransferObjectTo(IEu5Object obj, FileSavingWrapper targetFile)
    {
        targetFile.AddObject(obj);
        _newObjects.Remove(obj);
        _newFileQuaestor.RemoveFromIndex(obj);
        ShownObjects.Remove(obj);
    }

    private Eu5FileObj GetFileFromItem(object sender)
    {
        if (FileListView.ContainerFromElement((DependencyObject)sender) is ListViewItem container)
        {
            return (Eu5FileObj)container.Content; // Source object
        }

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
            MBox.Show("Please select a object first to create a new file for it.", "No object selected", icon: MessageBoxImage.Warning, owner:this);
            return;
        }

        var dialog = new CreateNewFile(_currentDescriptor, _savingWrapperManager)
        {
            Owner = this
        };
        dialog.ShowDialog();
        if (dialog.DialogResult != true)
            return;
        var newFile = _savingWrapperManager.GetFile(new(dialog.NewPath, _currentDescriptor));
        var descriptor = _savingWrapperManager.GetDescriptor(_currentDescriptor);
        descriptor.AddNewFile(newFile);
        ShownFiles = new(descriptor.AllFiles);
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
        if (e.Key != Key.Enter) return;
        if (ResultView.SelectedItem is ISearchable selected)
            HighlightSearchResult(selected);
    }

    private void HighlightSearchResult(ISearchable searchable)
    {
        if (searchable is IEu5Object)
        {
            var firstOrDefault = ShownObjects.FirstOrDefault(x => x!.Equals(searchable), null);
            if(firstOrDefault is null)
                return;
            ObjectListView.SelectedItem = firstOrDefault;
        }
        SearchResult.Clear();
    }
}