using Arcanum.UI.Commands;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class CommandBindingTest
{
   public PopupViewModel ViewModel { get; } = new();

   public CommandBindingTest()
   {
      DataContext = ViewModel;
      InitializeComponent();
   }
}

public class PopupViewModel
{
   public IAppCommand CloseCommand { get; } = CommandRegistry.Get(CommandIds.UI.Window.Close);
}