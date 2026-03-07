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
   public IAppCommand OpenFeatureCommand { get; } = CommandRegistry.Get(CommandIds.HelpWindow.OpenFeatureExplorerForFeature);

   // 2. Discoverability
   public IAppCommand RandomTip
   {
      get;
      set
      {
         if (Equals(value, field))
            return;

         field = value;
         OnPropertyChanged();
      }
   }
   private readonly IAppCommand[] _featureCommands;
   private int _currentTipIndex;
   public IAppCommand NextTipCommand { get; } = CommandRegistry.Get(CommandIds.HelpWindow.DashBoardView.NextRandomTip);
   public IAppCommand PreviousTipCommand { get; } = CommandRegistry.Get(CommandIds.HelpWindow.DashBoardView.PreviousRandomTip);
   public ObservableCollection<IAppFeature> LatestFeatures { get; } = [];

   public DashboardViewModel()
   {
      // Find features introduced in the latest version
      var latest = FeatureRegistry.GetAllFeatures()
                                  .OrderByDescending(f => f.IntroducedIn)
                                  .Take(3);
      foreach (var f in latest)
         LatestFeatures.Add(f);

      _featureCommands = CommandRegistry.AllCommands.Shuffle().ToArray();
      RandomTip = _featureCommands[_currentTipIndex = new Random().Next(_featureCommands.Length)];
   }

   private IEnumerable<IAppCommand> GetContextCommands()
   {
      if (ActiveFeature == null)
         return [];

      return CommandRegistry.AllCommands.Where(c => ActiveFeature.AssociatedScopes.Contains(c.Scope)).Take(5);
   }

   // Command to open URLs
   public static ICommand OpenUrlCommand => new RelayCommand(url => ProcessHelper.OpenLink(url?.ToString() ?? string.Empty));

   public void ShowPreviousTip()
   {
      if (_featureCommands.Length == 0)
         return;

      if (--_currentTipIndex < 0)
         _currentTipIndex = _featureCommands.Length - 1;
      else if (_currentTipIndex >= _featureCommands.Length)
         _currentTipIndex = 0;

      RandomTip = _featureCommands[_currentTipIndex];
   }

   public void ShowNextTip()
   {
      if (_featureCommands.Length == 0)
         return;

      if (++_currentTipIndex >= _featureCommands.Length)
         _currentTipIndex = 0;
      else if (_currentTipIndex < 0)
         _currentTipIndex = _featureCommands.Length - 1;

      RandomTip = _featureCommands[_currentTipIndex];
   }
}