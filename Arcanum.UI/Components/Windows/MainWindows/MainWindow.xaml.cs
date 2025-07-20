using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Arcanum.Core.FlowControlServices;
using Arcanum.Core.Globals;
using Arcanum.UI.Components.MVVM.Views.MainWindow;
using Application = System.Windows.Application;

namespace Arcanum.UI.Components.Windows.MainWindows;

public partial class MainWindow
{
   public const int DEFAULT_WIDTH = 1920;
   public const int DEFAULT_HEIGHT = 1080;

   private readonly MainWindowView _view;

   public MainWindow()
   {
      InitializeComponent();

      _view = DataContext as MainWindowView ??
              throw new InvalidOperationException("DataContext is not set or is not of type MainWindowView.");
   }

   public void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      Close();
   }

   public void GoToArcanumMenuScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
   {
      var menuWindow = Application.Current.Windows.OfType<MainMenuScreen>().FirstOrDefault();
      if (menuWindow == null)
         throw new InvalidOperationException("MainMenuScreen window not found.");

      menuWindow.MainViewModel.TargetedView = MainMenuScreen.MainMenuScreenView.Arcanum;
      Close();
   }

   private void CommandCanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

   private void ExitArcanum_OnExecuted(object sender, ExecutedRoutedEventArgs e)
   {
      LifecycleManager.Instance.RunShutdownSequence();
      Application.Current.Shutdown();
   }

   private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
   {
      var interop = new WindowInteropHelper(this);
      var screen = Screen.FromHandle(interop.Handle);
      if (screen.Bounds.Height <= DEFAULT_HEIGHT || screen.Bounds.Width <= DEFAULT_WIDTH)
      {
         Height = screen.WorkingArea.Height * 0.8;
         Width = screen.WorkingArea.Width * 0.8;
         WindowState = WindowState.Maximized;
      }
   }
}