using System.IO;
using System.Text;
using System.Windows;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.GlobalStates;

namespace Arcanum.UI.Components.Windows.MinorWindows.CrashHandler;

public partial class CrashHandler
{
   private Exception _exception;

   public CrashHandler()
   {
      _exception = null!;
      InitializeComponent();
   }

   public static void Show(Exception exception)
   {
      var crashHandler = new CrashHandler { _exception = exception, };
      crashHandler.ShowDialog();
   }

   private void CloseButton_Click(object sender, RoutedEventArgs e)
   {
      Close();
   }

   private void SaveCrashLog(object sender, RoutedEventArgs e)
   {
      // Create the crash log content
      var sb = new StringWriter();

      sb.WriteLine("Application Crash Log");
      sb.WriteLine("=====================");
      sb.WriteLine($"Timestamp: {DateTime.Now}");
      sb.WriteLine();
      sb.WriteLine($"Application Version: {AppData.ProductName} {AppData.AppVersion}");
      sb.WriteLine();
      sb.WriteLine("Exception Details:");
      sb.WriteLine(_exception.ToString());
      sb.WriteLine();

      sb.WriteLine("Inner Exception:");
      sb.WriteLine(_exception.InnerException?.ToString() ?? "None");
      sb.WriteLine();

      sb.WriteLine("Stack Trace:");
      sb.WriteLine(_exception.StackTrace);
      sb.WriteLine();

      sb.WriteLine("User Description:");
      sb.WriteLine(CrashReportTextBox.Text);
      sb.WriteLine();

      sb.WriteLine("Loaded Assemblies:");
      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
         sb.WriteLine($"{assembly.FullName} - {assembly.Location}");
      sb.WriteLine();

      sb.WriteLine("Command History:");
      var (add, rmv) = AppData.HistoryManager.GetPathToCurrent();
      sb.WriteLine($"Added Paths: {string.Join(", ", add)}");
      sb.WriteLine($"Removed Paths: {string.Join(", ", rmv)}");
      sb.WriteLine();

      sb.WriteLine("System Information:");
      sb.WriteLine($"OS Version: {Environment.OSVersion}");
      sb.WriteLine($"Processor Count: {Environment.ProcessorCount}");
      sb.WriteLine($"CLR Version: {Environment.Version}");

      var logContent = sb.ToString();

      IO.WriteAllText(Path.Combine(IO.GetCrashLogsPath, $"crash_{DateTime.Now.ToFileTime()}.log"),
                      logContent,
                      Encoding.UTF8);
   }
}