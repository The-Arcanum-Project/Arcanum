using System.Collections.ObjectModel;
using Arcanum.UI.Components.UserControls.SaveWindow;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SaveWindow
{
    public SaveWindow()
    {
        InitializeComponent();

        // Load the data directly here.
        LoadData();
        // Set the DataContext of the window to itself, so the XAML bindings work.
        this.DataContext = this;
    }


        
    private void LoadData()
    {
        FilesStackPanel.Children.Add(new FileDragDropItem("example.txt", new List<SaveableItem>(
            new[]
            {
                new SaveableItem ("Country A"),
                new SaveableItem ("Country B"),
                new SaveableItem ("Country C")
            }), ItemTag.TagA
            ));
        
        FilesStackPanel.Children.Add(new FileDragDropItem("example2.txt", new List<SaveableItem>(
            new[]
            {
                new SaveableItem ("Province A1"),
                new SaveableItem ("Province B1"),
                new SaveableItem ("Province C1")
            }), ItemTag.TagB));
        
        FilesStackPanel.Children.Add(new FileDragDropItem("example2.txt", new List<SaveableItem>(
            new[]
            {
                new SaveableItem ("Country A1"),
                new SaveableItem ("Country B1"),
                new SaveableItem ("Country C1")
            }), ItemTag.TagA));
    }

}