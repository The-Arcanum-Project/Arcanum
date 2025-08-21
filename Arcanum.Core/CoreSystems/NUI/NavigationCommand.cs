using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// This command is used in all NUI navigation scenarios.
/// </summary>
/// <param name="nuiRoot"></param>
/// <param name="getTargetView"></param>
/// <param name="canExecute"></param>
public sealed class NavigationCommand(ContentPresenter nuiRoot,
                                      Func<UserControl> getTargetView,
                                      Func<bool>? canExecute = null)
   : ICommand
{
   /// <summary>
   /// by default, this command can always execute.
   /// </summary>
   private readonly Func<bool> _canExecute = canExecute ?? (() => true);

   /// <summary>
   /// The root of the NUI system that is used to set the view.
   /// </summary>
   private readonly ContentPresenter _nuiRoot = nuiRoot ?? throw new ArgumentNullException(nameof(nuiRoot));

   /// <summary>
   /// The Function which returns the target view to be set in the NUIRoot.
   /// </summary>
   private readonly Func<UserControl> _getTargetView =
      getTargetView ?? throw new ArgumentNullException(nameof(getTargetView));

   public bool CanExecute(object? parameter) => _canExecute();

   public void Execute(object? parameter) => _nuiRoot.Content = _getTargetView.Invoke();

   public event EventHandler? CanExecuteChanged;
}