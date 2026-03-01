using System.Collections.ObjectModel;
using System.Windows.Input;
using Arcanum.UI.AppFeatures;
using Arcanum.UI.Commands;
using Arcanum.UI.Components.Windows.DebugWindows;
using Common;

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public class DashboardViewModel : HelpPageViewModelBase
{
   public override string Title => "Knowledge Dashboard";

   // 1. Live Context
   public IAppFeature? ActiveFeature => FeatureRegistry.GetActiveFeatures().LastOrDefault();
   public IEnumerable<IAppCommand> ContextCommands => GetContextCommands();

   // 2. Discoverability
   public IAppCommand RandomTip { get; private set; }
   public ObservableCollection<IAppFeature> LatestFeatures { get; } = [];

   public DashboardViewModel()
   {
      // Find features introduced in the latest version
      var latest = FeatureRegistry.GetAllFeatures()
                                  .OrderByDescending(f => f.IntroducedIn)
                                  .Take(3);
      foreach (var f in latest)
         LatestFeatures.Add(f);

      // Pick a random command to teach the user something new
      var allCmds = CommandRegistry.AllCommands.ToList();
      RandomTip = allCmds[new Random().Next(allCmds.Count)];
   }

   private IEnumerable<IAppCommand> GetContextCommands()
   {
      if (ActiveFeature == null)
         return [];

      return CommandRegistry.AllCommands.Where(c => ActiveFeature.AssociatedScopes.Contains(c.Scope)).Take(5);
   }

   // Command to open URLs
   public ICommand OpenUrlCommand => new RelayCommand(url => ProcessHelper.OpenLink(url?.ToString() ?? string.Empty));
}