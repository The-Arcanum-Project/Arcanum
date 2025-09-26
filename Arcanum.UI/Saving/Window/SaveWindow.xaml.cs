using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GlobalStates;
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
/// Once an object is dragged, all valid files appear to drag them into.
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
    private Point? _dragStartPoint = null;
    
    private readonly FileSearchSettings _settings = new();
    private readonly Queastor _quaestor;
    private readonly List<IEu5Object> _newObjects;
    private readonly Dictionary<Eu5FileObj, FileRepresentation> _fileDescriptors = new();
    
    #region UI Bindings

    public static readonly DependencyProperty ShownFilesProperty = DependencyProperty.Register(
        nameof(ShownFiles), typeof(ObservableCollection<FileRepresentation>), typeof(SaveWindow),
        new(new ObservableCollection<FileRepresentation>([])));

    public ObservableCollection<FileRepresentation> ShownFiles
    {
        get => (ObservableCollection<FileRepresentation>)GetValue(ShownFilesProperty);
        set => SetValue(ShownFilesProperty, value);
    }

    public static readonly DependencyProperty ShownDescriptorsProperty = DependencyProperty.Register(
        nameof(ShownDescriptors), typeof(ObservableCollection<FileDescriptor>), typeof(SaveWindow), new(new ObservableCollection<FileDescriptor>([])));

    public ObservableCollection<FileDescriptor> ShownDescriptors
    {
        get => (ObservableCollection<FileDescriptor>)GetValue(ShownDescriptorsProperty);
        set => SetValue(ShownDescriptorsProperty, value);
    }

    public static readonly DependencyProperty ShownObjectsProperty = DependencyProperty.Register(
        nameof(ShownObjects), typeof(ObservableCollection<IEu5Object>), typeof(SaveWindow), new(new ObservableCollection<IEu5Object>([])));

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

    public SaveWindow(List<IEu5Object> newObjects)
    {
        _newObjects = newObjects;
        _quaestor = new (new());
        
        InitializeComponent();
        SetupUi();
    }

    private void SetupUi()
    {
        SearchBox.RequestSearch = SearchBoxRequestSearch;
        SearchBox.SettingsOpened = OpenSettingsWindow;
    }

    private void SetUpNewObjectMode()
    {
        ShownFiles.Clear();
        ShownObjects = new (_newObjects);
    }

    private void SetUpNormalMode()
    {
        ShownFiles.Clear();
        ShownObjects.Clear();
    }

    private void SearchBoxRequestSearch(string obj)
    {
        throw new NotImplementedException();
    }

    private void OpenSettingsWindow()
    {
        throw new NotImplementedException();
    }

    #region UI Events
    
    private void OnDropInFile(object sender, DragEventArgs e)
    {
        
    }
    
    private void OnDragInFile(object sender, DragEventArgs e)
    {
        
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

        var obj = GetObjectFromItem(sender);
        data.SetData("object", obj);
        _dragStartPoint = null;
        try
        {
            ShownFiles = new (GetDescriptors(obj.Source.Descriptor.Files));
            DragDrop.DoDragDrop(ObjectListView, data, DragDropEffects.Move);
        }
        finally
        {
            ShownFiles.Clear();
        }
    }

    private void NewObjectModeToggle_OnChecked(object sender, RoutedEventArgs e)
    {
        NewFileMode = true;
        DescriptorsColumn.MinWidth = 0;
        DescriptorsColumn.Width = new GridLength(0);
        SplitterColumn.Width = new GridLength(0);
        SetUpNewObjectMode();
    }

    private void NewObjectModeToggle_OnUnchecked(object sender, RoutedEventArgs e)
    {
        NewFileMode = false;
        DescriptorsColumn.MinWidth = 100;
        DescriptorsColumn.Width = new GridLength(1, GridUnitType.Star);
        SplitterColumn.Width = GridLength.Auto;
        SetUpNormalMode();
    }

    private void ObjectListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if(!NewFileMode) return;
        // Check if all new objects have a common descriptor
        if (ObjectListView.SelectedItems.Count < 1)
        {
            ShownFiles.Clear();
            return;
        }

        var selectedDescriptor = GetDescriptorFromItem(ObjectListView.SelectedItems[0] as IEu5Object ?? throw new InvalidOperationException());
        for (var index = 1; index < ObjectListView.SelectedItems.Count; index++)
        {
            var item = ObjectListView.SelectedItems[index];
            var descriptor = GetDescriptorFromItem(item as IEu5Object ?? throw new InvalidOperationException());
            if (Equals(descriptor, selectedDescriptor)) continue;
            ShownFiles.Clear();
            return;
        }
        ShownFiles = new (GetDescriptors(selectedDescriptor.Files));
    }
    #endregion

    #region Helper

    private IEu5Object GetObjectFromItem(object sender)
    {
        if (ObjectListView.ContainerFromElement((DependencyObject)sender) is ListViewItem container)
        {
            return (IEu5Object)container.Content; // Source object
        }

        throw new ArgumentException("Sender is not a ListViewItem");
    }

    private FileDescriptor GetDescriptorFromItem(IEu5Object obj)
    {
       return obj.Source.Descriptor;
    }

    private FileRepresentation GetDescriptor(Eu5FileObj fileObj)
    {
        if(!_fileDescriptors.TryGetValue(fileObj, out var descriptor))
            _fileDescriptors[fileObj] = descriptor = new(_quaestor, fileObj, []);
        return descriptor;
    }

    private List<FileRepresentation> GetDescriptors(List<Eu5FileObj> objects)
    {
        var list = new List<FileRepresentation>(objects.Count);
        list.AddRange(objects.Select(GetDescriptor));
        return list;
    }

    #endregion

}