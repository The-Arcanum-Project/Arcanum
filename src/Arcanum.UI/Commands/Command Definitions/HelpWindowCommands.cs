#region

using System.Windows.Input;
using Arcanum.Core.ApplicationContext;
using Arcanum.UI.Components.Windows.HelpWindow.ViewModels;
using Arcanum.UI.Documentation.Implementation;

#endregion

namespace Arcanum.UI.Commands.Command_Definitions;

public static class HelpWindowCommands
{
   public static void Initialize()
   {
      new ManagedCommand(CommandIds.HelpWindow.OpenFeatureExplorerForFeature,
                         "Open a Feature's Help Page",
                         "Opens the help page and focuses on the tab for the given feature, if it exists.",
                         CommandScopes.HELP_DASHBOARD,
                         param =>
                         {
                            if (param is FeatureDoc feature)
                               ArcAppContext.Get<IHelpPageViewModelWrapper>()?.ActivateFeatureTabFor(feature);
                         },
                         canExecute: _ => ArcAppContext.Has<IHelpPageViewModelWrapper>()).WithDefaultGestures([]);

      // "help_window.dashboard_view.next_tip"
      new ManagedCommand(CommandIds.HelpWindow.DashBoardView.NextRandomTip,
                         "Next Tip",
                         "Shows the next tip in the dashboard view.",
                         CommandScopes.HELP_DASHBOARD,
                         _ => ArcAppContext.Get<IHelpPageViewModelWrapper>()?.ShowNextTip(),
                         canExecute: _ => ArcAppContext.Has<IHelpPageViewModelWrapper>()).WithDefaultGestures([(Key.Right, ModifierKeys.Alt)]);

      // "help_window.dashboard_view.previous_tip"
      new ManagedCommand(CommandIds.HelpWindow.DashBoardView.PreviousRandomTip,
                         "Previous Tip",
                         "Shows the previous tip in the dashboard view.",
                         CommandScopes.HELP_DASHBOARD,
                         _ => ArcAppContext.Get<IHelpPageViewModelWrapper>()?.ShowPreviousTip(),
                         canExecute: _ => ArcAppContext.Has<IHelpPageViewModelWrapper>()).WithDefaultGestures([(Key.Left, ModifierKeys.Alt)]);
   }
}