using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.UI.Components.StyleClasses;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arcanum.UI.Saving.Window;

public partial class CreateNewFile
{

    public PathObj NewPath = PathObj.Empty;

    public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
        nameof(FileName), typeof(string), typeof(CreateNewFile), new PropertyMetadata(default(string)));
    
    public FileDescriptor FileDescriptor { get; }
    
    public string FileName
    {
        get { return (string)GetValue(FileNameProperty); }
        set { SetValue(FileNameProperty, value); }
    }
    
    public CreateNewFile(FileDescriptor fileDescriptor)
    {
        FileDescriptor = fileDescriptor;
        InitializeComponent();

        var binding = new Binding("FileName")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(CreateNewFile), 1),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        var validationRule = new WindowsFileNameValidation
        {
            Descriptor = FileDescriptor,
            ValidatesOnTargetUpdated = true
        };
        binding.ValidationRules.Add(validationRule);

        FileNameInput.SetBinding(TextBox.TextProperty, binding);
        
        FilePath.Text = fileDescriptor.FilePath;
        FileExtension.Text = '.' + fileDescriptor.FileType.FileEnding;
        FileNameInput.Focus();
    }
    
    private void FileNameInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        // Force the binding to update the source
        FileNameInput.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

        // Check for validation errors
        if (!Validation.GetHasError(FileNameInput))
        {
            DialogResult = true; // close the window with success
            Close();
        }
        else
        {
            // Optionally, show a message or just let the user correct the input
        }

        e.Handled = true; // prevent default behavior
    }
    
}