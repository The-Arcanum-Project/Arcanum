using System.ComponentModel;

namespace Arcanum.UI.Components.Windows.PopUp;

public partial class SavingSplashScreen
{
   private bool _canClose;
   private int? _totalFiles;

   public SavingSplashScreen(int? totalFilesToSave = null)
   {
      InitializeComponent();
      _totalFiles = totalFilesToSave;

      SetupProgressBar();
   }

   private void SetupProgressBar()
   {
      if (_totalFiles is > 0)
      {
         // Deterministic Mode
         SaveProgressBar.IsIndeterminate = false;
         SaveProgressBar.Maximum = _totalFiles.Value;
         SaveProgressBar.Value = 0;
      }
      else
         // Indeterminate Mode (Unknown number of files)
         SaveProgressBar.IsIndeterminate = true;
   }

   public void UpdateProgress(string fileName)
   {
      Dispatcher.Invoke(() =>
      {
         CurrentFileText.Text = $"Saving: {fileName}";

         if (!_totalFiles.HasValue)
            return;

         if (SaveProgressBar.Value < SaveProgressBar.Maximum)
            SaveProgressBar.Value++;
      });
   }

   public void SetTotalFiles(int totalFiles)
   {
      Dispatcher.Invoke(() =>
      {
         _totalFiles = totalFiles;
         SetupProgressBar();
      });
   }

   public void MarkAsComplete()
   {
      _canClose = true;
      Dispatcher.Invoke(Close);
   }

   protected override void OnClosing(CancelEventArgs e)
   {
      if (!_canClose)
         e.Cancel = true;

      base.OnClosing(e);
   }
}