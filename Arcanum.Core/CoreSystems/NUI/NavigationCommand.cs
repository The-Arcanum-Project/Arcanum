using System.Windows.Input;

namespace Arcanum.Core.CoreSystems.NUI;

/// <summary>
/// This command is used in all NUI navigation scenarios.
/// </summary>
/// <param name="nuiRoot"></param>
/// <param name="getTargetView"></param>
/// <param name="canExecute"></param>
public sealed class NavigationCommand(NUIRoot nuiRoot,
                                      Func<NUIUserControl> getTargetView,
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
   private readonly NUIRoot _nuiRoot = nuiRoot ?? throw new ArgumentNullException(nameof(nuiRoot));

   /// <summary>
   /// The Function which returns the target view to be set in the NUIRoot.
   /// </summary>
   private readonly Func<NUIUserControl> _getTargetView =
      getTargetView ?? throw new ArgumentNullException(nameof(getTargetView));

   public bool CanExecute(object? parameter) => _canExecute();

   public void Execute(object? parameter) => _nuiRoot.SetView(_getTargetView.Invoke());

   public event EventHandler? CanExecuteChanged;
}