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
      "Press `F10` to view the error log", "Press `F1` to open the settings", "Press `Ctrl + F` to open the application wide search: \"Queastor\"",
      "Are you missing a feature? Open an issue on GitHub or join the Discord server to see if a member of the community created a plugin for it!",
      "You can customize which properties of each element are shown in the settings.",
      "Backing up your mod files before making changes is always a good idea!",
      "Are teh popups of errors annoying? You can disable them in the settings individually or all at once.",
      "Press `C` to toggle the brush tool to paint a selection on the map.",
      "You can edit as many objects as you want at once! Just select them all and edit their properties in the sidebar.",
      "With `RMB` on a map mode button you can change the mapmode assigned to it.",
      "Working on several mods at once? You can access recently opened mods from the main menu!",
      "You don't want to edit location data but climates/religions/cultures instead? Just switch the map mode and use `infer selection` mode!",
      "You have errors in your mod? Use the Error Log (`F10`) to view and the \"navigat to file\" button on the bottom right to open the file at the errors location!",
   ];

   private DispatcherTimer? _tipTimer;
   private readonly Random _random = new();

   public string LoadingTip
   {
      get;
      set
      {
         if (field != value)
         {
            field = value;
            OnPropertyChanged();
         }
      }
   } = "Just do it right the first time!";

   public string LoadingText
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         OnPropertyChanged();
      }
   } = "Starting...";

   private string _stepName = "Step: Initializing...";
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