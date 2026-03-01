namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class ColorPickerWindow
{
   public ColorPickerWindow()
   {
      InitializeComponent();
      if (Owner is { } ownerWindow)
         Topmost = ownerWindow.Topmost;
   }
}