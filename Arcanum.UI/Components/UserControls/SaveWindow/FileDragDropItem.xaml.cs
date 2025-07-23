using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.SaveWindow;

public class SaveableItem(string name)
{
    public string Name { get; init; } = name;
    public ItemTag TagGroup { get; set; }
}

public partial class FileDragDropItem
{
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items),
        typeof(IList<SaveableItem>), typeof(FileDragDropItem), new PropertyMetadata(default(IList<SaveableItem>)));

    public FileDragDropItem(string fileName, IList<SaveableItem> items, ItemTag tagGroup)
    {
        Items = new(items);
        TagGroup = tagGroup;
        FileName = fileName;
        InitializeComponent();
    }

    // Register the FileName property to bind it in XAML
    public static readonly DependencyProperty FileNameProperty =
        DependencyProperty.Register(nameof(FileName), typeof(string), typeof(FileDragDropItem),
            new PropertyMetadata(string.Empty));

    public string FileName
    {
        get => (string)GetValue(FileNameProperty);
        init => SetValue(FileNameProperty, value);
    }

    public ObservableCollection<SaveableItem> Items
    {
        get => (ObservableCollection<SaveableItem>)GetValue(ItemsProperty);
        init => SetValue(ItemsProperty, value);
    }

    public static readonly DependencyProperty TagFileProperty =
        DependencyProperty.Register(nameof(TagGroup), typeof(ItemTag), typeof(FileDragDropItem),
            new PropertyMetadata(ItemTag.TagA));

    public ItemTag TagGroup
    {
        get => (ItemTag)GetValue(TagFileProperty);
        init => SetValue(TagFileProperty, value);
    }

    private void ItemListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement fe || fe.DataContext == null) return;
        if (ListBox.ContainerFromElement(fe) is not ListBoxItem { Content: SaveableItem saveableItem }) return;
        var data = new DataObject();

        saveableItem.TagGroup = TagGroup;

        HideOtherGroups(saveableItem.TagGroup);
        data.SetData(typeof(SaveableItem), saveableItem);
        data.SetData("source", this);
        try
        {
            DragDrop.DoDragDrop(ListBox, data, DragDropEffects.Move);
        }
        finally
        {
            ReShowGroups();
        }
    }

    private void ReShowGroups()
    {
        if (Parent is not Panel parentPanel) return;

        foreach (var child in parentPanel.Children)
        {
            if (child is FileDragDropItem item)
            {
                item.IsEnabled = true;
                //item.Visibility =  Visibility.Visible;
            }
        }
    }
    

    private void HideOtherGroups(ItemTag tag)
    {
        if (Parent is not Panel parentPanel) return;

        foreach (var child in parentPanel.Children)
        {
            if (child is FileDragDropItem item && item.TagGroup != tag)
            {
                //item.Visibility = tag.Equals(item.TagGroup) ? Visibility.Visible : Visibility.Collapsed;
                item.IsEnabled = false;
            }
        }
    }


    private void DropTargetBorder_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(SaveableItem)) && !Equals(e.Data.GetData("source"))
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void DropTargetBorder_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(SaveableItem)) && !Equals(e.Data.GetData("source"))
            ? DragDropEffects.Move
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void DropTargetBorder_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(SaveableItem))) return;
        if (e.Data.GetData(typeof(SaveableItem)) is not SaveableItem myData)
            return;

        Items.Add(myData);
        if (e.Data.GetData("source") is FileDragDropItem source && source.Items.Contains(myData))
        {
            source.Items.Remove(myData);
        }
    }
}

public enum ItemTag
{
    TagA,
    TagB,
    TagC,
}