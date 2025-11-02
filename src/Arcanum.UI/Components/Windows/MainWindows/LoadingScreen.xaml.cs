using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

namespace Arcanum.UI.Components.Windows.MainWindows;

public partial class LoadingScreen : INotifyPropertyChanged
{
   public LoadingScreen()
   {
      InitializeComponent();

      DataContext = this; // Needed for bindings

      Loaded += (_, _) =>
      {
         _tipTimer = new() { Interval = TimeSpan.FromSeconds(5) };
         _tipTimer.Tick += (_, _) => UpdateTip();
         _tipTimer.Start();
         UpdateTip();
      };
   }

   private readonly string[] _loadingTips =
   [
      "Press `F10` to view the error log", "Press `F1` to open the settings",
      "Press `Ctrl + F` to open the application wide search: \"Queastor\"",
      "Are you missing a feature? Open an issue on GitHub or join the Discord server to see if a member of the community created a plugin for it!",
   ];

   private DispatcherTimer? _tipTimer;
   private readonly Random _random = new();

   private string _loadingTip = "Just do it right the first time!";
   public string LoadingTip
   {
      get => _loadingTip;
      set
      {
         if (_loadingTip != value)
         {
            _loadingTip = value;
            OnPropertyChanged();
         }
      }
   }

   private string _loadingText = "Starting...";
   public string LoadingText
   {
      get => _loadingText;
      set
      {
         if (_loadingText == value)
            return;

         _loadingText = value;
         OnPropertyChanged();
      }
   }

   private string _stepName = "Step: Initializing...";
   private TimeSpan _estimatedTime = TimeSpan.Zero;
   private double _totalProgressPercentage;

   public async Task<bool> StartLoading()
   {
      ParsingMaster.Instance.ParsingStepsChanged += (_, step) =>
      {
         _stepName = step.Name;
         FormatLoadingText();
      };

      ParsingMaster.Instance.TotalProgressChanged += (_, percentage) =>
      {
         _totalProgressPercentage = percentage;
         FormatLoadingText();
      };

      return await Task.Run(() => ParsingMaster.Instance.ExecuteAllParsingSteps());
   }

   private void FormatLoadingText()
   {
      // First, construct the string. This can be done on any thread.
      var newText =
         $"{_stepName} {_totalProgressPercentage:F0}% ({ParsingMaster.Instance.ParsingStepsDone}/{ParsingMaster.Instance.ParsingSteps})";

      // Now, use the Dispatcher to set the property on the UI thread.
      // This is the crucial part.
      Dispatcher.InvokeAsync(() => { LoadingText = newText; });
   }

   private void UpdateTip()
   {
      LoadingTip = _loadingTips[_random.Next(_loadingTips.Length)];
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}