using System.ComponentModel;
using System.Windows.Threading;
using Arcanum.UI.Components.Models;

namespace Arcanum.UI.Components.Views.LoadingScreen;

public class LoadingScreenView : INotifyPropertyChanged
{
   private readonly string[] _loadingTips =
   [
      "Press `F10` to view the error log", "Press `F1` to view the console",
      "Press `Shift + Shift` to open the application wide search: \"Queastor\"",
      "Are you missing a feature? Open an issue on GitHub or join the Discord server to see if a member of the community created a plugin for it!",
   ];

   private readonly DispatcherTimer _tipTimer;
   private readonly Random _random = new();

   private readonly LoadingScreenModel _model;

   private string _loadingTip = "Just do it right the first time!";
   public string LoadingTip
   {
      get => _loadingTip;
      set
      {
         if (_loadingTip != value)
         {
            _loadingTip = value;
            PropertyChanged?.Invoke(this, new(nameof(LoadingTip)));
         }
      }
   }

   private string _loadingText = "Starting...";
   public string LoadingText
   {
      get => _loadingText;
      set
      {
         if (_loadingText != value)
         {
            _loadingText = value;
            PropertyChanged?.Invoke(this, new(nameof(LoadingText)));
         }
      }
   }

   public LoadingScreenView(LoadingScreenModel model)
   {
      _model = model;
      _model.ProgressChanged += text => LoadingText = text;

      _tipTimer = new() { Interval = TimeSpan.FromSeconds(5) };
      _tipTimer.Tick += (_, _) => UpdateTip();
      _tipTimer.Start();
      UpdateTip();
   }

   public async Task StartLoading()
   {
      await _model.RunLoadingAsync();
   }

   private void UpdateTip()
   {
      LoadingTip = _loadingTips[_random.Next(_loadingTips.Length)];
   }

   public event PropertyChangedEventHandler? PropertyChanged;
}