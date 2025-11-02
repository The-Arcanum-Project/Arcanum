using System.Windows;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class UIElementsBrowser
{
   public UIElementsBrowser()
   {
      InitializeComponent();
   }
#if DEBUG
   private object _selectedObject = new AllOptionsTestObject();
#else
    private object _selectedObject = new YourDataType("Test");
#endif

   public object SelectedObject
   {
      get => _selectedObject;
      set => _selectedObject = value;
   }

   public int IntValue { get; set; } = 42;
   public decimal DecimalValue { get; set; } = 3.14m;
   public float FloatValue { get; set; } = 2.718f;

   private void ShowMessageBoxOkCancel_Click(object sender, RoutedEventArgs e)
   {
      MBox.Show("This is a test message box.",
                "Test Message Box",
                MBoxButton.OKCancel,
                MessageBoxImage.Information);
   }

   private void ShowMessageBoxOkRetryCancel_Click(object sender, RoutedEventArgs e)
   {
      MBox.Show("This is a test message box with OK, Retry, and Cancel buttons.",
                "Test Message Box",
                MBoxButton.OKRetryCancel,
                MessageBoxImage.Warning);
   }

   private void ShowMessageBoxOkRetry_Click(object sender, RoutedEventArgs e)
   {
      MBox.Show("This is a test message box with OK and Retry buttons.",
                "Test Message Box",
                MBoxButton.OKRetry,
                MessageBoxImage.Error);
   }

   private void ShowMessageBoxOk_Click(object sender, RoutedEventArgs e)
   {
      MBox.Show("This is a test message box with only an OK button.",
                "Test Message Box",
                MBoxButton.OK,
                MessageBoxImage.Information);
   }
}

public class YourDataType(string str)
{
   public YourDataType() : this(string.Empty)
   {
   }

   public object Column1 { get; set; } = $"Column 1 {str}";
   public object Column2 { get; set; } = $"Column 2 {str}";
   public object Column3 { get; set; } = $"Column 3 {str}";
}