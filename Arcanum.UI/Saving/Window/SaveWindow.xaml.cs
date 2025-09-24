using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.Windows.PopUp;
using Arcanum.UI.Saving.Backend;

namespace Arcanum.UI.Saving.Window;

public partial class SaveWindow
{
    private readonly FileSearchSettings _settings = new();
    private readonly Queastor _quaestor;
    private List<FileRepresentation> _selectedFiles = [];
    private bool _ignoreSelectionEvent = false;

    public static readonly DependencyProperty SearchResultsProperty = DependencyProperty.Register(
        nameof(SearchResults), typeof(ObservableCollection<FileRepresentation>), typeof(SaveWindow),
        new PropertyMetadata(default(ObservableCollection<FileRepresentation>)));

    public ObservableCollection<FileRepresentation> SearchResults
    {
        get { return (ObservableCollection<FileRepresentation>)GetValue(SearchResultsProperty); }
        set { SetValue(SearchResultsProperty, value); }
    }

    public static readonly DependencyProperty CurrentFileProperty = DependencyProperty.Register(
        nameof(CurrentFile), typeof(FileRepresentation), typeof(SaveWindow),
        new PropertyMetadata(default(FileRepresentation)));

    public FileRepresentation CurrentFile
    {
        get { return (FileRepresentation)GetValue(CurrentFileProperty); }
        set { SetValue(CurrentFileProperty, value); }
    }

    private readonly List<FileRepresentation> _files;

    public SaveWindow()
    {
        InitializeComponent();
        _quaestor = new(new QueastorSearchSettings());

        _files = DescriptorDefinitions.RegenciesDescriptor.Files
            .Select(f => new FileRepresentation(_quaestor, f, f.ObjectsInFile.ToList())).ToList();
        _files.AddRange(
            DescriptorDefinitions.ClimateDescriptor.Files.Select(f =>
                new FileRepresentation(_quaestor, f, f.ObjectsInFile.ToList())));
        _quaestor.RebuildBkTree();

        SearchResults = new(_files);

        BindQuaestor();

        FileListView.SelectionChanged += HandleFileSelectionChanged;
        SelectButton.Click += (_, _) => FileListView.SelectAll();
        SelectButton.MouseDoubleClick += (_, e) =>
        {
            _selectedFiles = _files;
            e.Handled = true;
        };
        UnselectButton.Click += (_, _) => FileListView.UnselectAll();
        UnselectButton.MouseDoubleClick += (_, e) =>
        {
            _selectedFiles = [];
            e.Handled = true;
        };
    }


    private void HandleFileSelectionChanged(object s, SelectionChangedEventArgs e)
    {
        if (_ignoreSelectionEvent) return;

        if (FileListView.SelectedItem is FileRepresentation file) SelectFile(file);

        _selectedFiles.AddRange(e.AddedItems.Cast<FileRepresentation>());
        foreach (var removedItem in e.RemovedItems.Cast<FileRepresentation>()) _selectedFiles.Remove(removedItem);
    }

    public void BindQuaestor()
    {
        SearchBox.RequestSearch = SearchBoxRequestSearch;
        SearchBox.SettingsOpened = OpenSettingsWindow;
    }

    private void SearchBoxRequestSearch(string s)
    {
        _ignoreSelectionEvent = true;

        if (string.IsNullOrWhiteSpace(s))
            SearchResults = new(_files);
        else
            SearchResults = new(_quaestor.Search(s).Cast<FileRepresentation>().Where(file =>
                _settings.AvailableCategories.Select(category => ((Eu5ObjectsRegistry.Eu5ObjectsEnum)category).ToType())
                    .Any(type => file.FileObj.Descriptor.LoadingService[0].ParsedObjects.Contains(type))));
        // Mark selected files again
        ReloadSelection();
    }

    private void ReloadSelection()
    {
        _ignoreSelectionEvent = true;
        foreach (var file in _selectedFiles.Where(file => SearchResults.Contains(file)))
        {
            FileListView.SelectedItems.Add(file);
        }

        _ignoreSelectionEvent = false;
    }

    private void OpenSettingsWindow()
    {
        var settingsPropWindow =
            new PropertyGridWindow(_settings)
            {
                Title = "Search Settings", WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
        settingsPropWindow.ShowDialog();
        _settings.ApplySettings(_quaestor.Settings);
    }

    private void SelectFile(FileRepresentation file)
    {
        CurrentFile = file;
    }

    private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var data = new DataObject();
        var obj = GetObjectFromItem(sender);
        if (!Eu5ObjectsRegistry.TryGetEnumRepresentation(obj.GetType(), out var result))
            throw new InvalidOperationException("The object type was not registered in the Eu5ObjectsRegistry");

        data.SetData("object", obj);
        data.SetData("type", result);

        try
        {
            DragDrop.DoDragDrop(ObjectListView, data, DragDropEffects.Move);
        }
        finally
        {
            //ReShowGroups();
        }
    }

    private IEu5Object GetObjectFromItem(object sender)
    {
        if (ObjectListView.ContainerFromElement((DependencyObject)sender) is ListViewItem container)
        {
            return (IEu5Object)container.Content; // Source object
        }

        return null!;
    }

    private FileRepresentation GetFileRepresentationFromItem(object sender)
    {
        if (FileListView.ContainerFromElement((DependencyObject)sender) is ListViewItem container)
        {
            return (FileRepresentation)container.Content; // Source object
        }

        return null!;
    }

    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        var item = GetFileRepresentationFromItem(sender);

        var eu5Object = e.Data.GetData("object") as IEu5Object;

        item.ChangedObjects.Add(eu5Object!);
        CurrentFile.ChangedObjects.Remove(eu5Object!);
    }

    private void UIElement_OnDrag(object sender, DragEventArgs e)
    {
        var item = GetFileRepresentationFromItem(sender);

        if (e.Data.GetData("type") is not Eu5ObjectsRegistry.Eu5ObjectsEnum enumType)
            throw new InvalidOperationException("The object type was not registered in the Eu5ObjectsRegistry");


        e.Effects = item.Equals(CurrentFile) ||
                    !item.AllowedObjects.Contains<Eu5ObjectsRegistry.Eu5ObjectsEnum>(enumType)
            ? DragDropEffects.None
            : DragDropEffects.Move;

        e.Handled = true;
    }
}