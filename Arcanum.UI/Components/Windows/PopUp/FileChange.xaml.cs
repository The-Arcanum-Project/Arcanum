using System.Collections.ObjectModel;
using System.Windows;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;

namespace Arcanum.UI.Components.Windows.PopUp;

public partial class FileChange
{

    public static readonly DependencyProperty propertyNameProperty = DependencyProperty.Register(
        nameof(propertyName), typeof(ObservableCollection<FileChangedEventArgs>), typeof(FileChange), new PropertyMetadata(default(ObservableCollection<FileChangedEventArgs>)));

    public ObservableCollection<FileChangedEventArgs> propertyName
    {
        get { return (ObservableCollection<FileChangedEventArgs>)GetValue(propertyNameProperty); }
        set { SetValue(propertyNameProperty, value); }
    }
    
    public FileChange()
    {
        InitializeComponent();
    }
}