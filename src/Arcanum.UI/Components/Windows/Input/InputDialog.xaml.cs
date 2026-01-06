using System.Globalization;
using System.Windows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.Windows.Input;

public enum InputKind
{
   String,
   Int,
   Float,
}

public partial class InputDialog
{
   private readonly InputKind _kind;

   public InputDialog(string title, string description, InputKind kind)
   {
      InitializeComponent();

      Title = title;
      DescriptionText.Text = description;
      _kind = kind;
   }

   public object? Value { get; private set; }

   private void OkClicked(object sender, RoutedEventArgs e)
   {
      var text = InputBox.Text;

      var success = _kind switch
      {
         InputKind.String => Set(text),
         InputKind.Int => int.TryParse(text, out var i) && Set(i),
         InputKind.Float => float.TryParse(text,
                                           NumberStyles.Float,
                                           CultureInfo.InvariantCulture,
                                           out var f) &&
                            Set(f),
         _ => false,
      };

      if (!success)
      {
         MBox.Show("Invalid input value.", "Error", MBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      DialogResult = true;
   }

   private bool Set(object value)
   {
      Value = value;
      return true;
   }
}