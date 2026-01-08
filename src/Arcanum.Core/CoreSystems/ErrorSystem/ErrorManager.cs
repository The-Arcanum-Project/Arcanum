using System.Text;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem;
using Common;

namespace Arcanum.Core.CoreSystems.ErrorSystem;

public static class ErrorManager
{
   public static List<Diagnostic> Diagnostics { get; } = [];

   public static void ClearLog()
   {
      Diagnostics.Clear();
      //Debug.WriteLine("---------------------------------\nCleared diagnostics log.\n---------------------------------");
   }

   public static void AddToLog(Diagnostic? diagnostic)
   {
      if (diagnostic == null)
         return;

      Diagnostics.Add(diagnostic);
      //Debug.WriteLine($"Added diagnostic: {diagnostic}");
   }

   public static void AddToLog(List<Diagnostic> diagnostics)
   {
      if (diagnostics.Count == 0)
         return;

      foreach (var diagnostic in diagnostics)
         Diagnostics.Add(diagnostic);
   }

   public static void PrintDiagnosticsToConsole(bool clean)
   {
      var sb = new StringBuilder();
      if (clean)
         Console.Clear();
      else
      {
         sb.AppendLine("");
         sb.AppendLine("");
      }

      ExportHumanReadableLog(sb);
      ArcLog.WritePure(sb.ToString());
   }

   public static void ExportHumanReadableLog(StringBuilder sb)
   {
      sb.AppendLine("########################################");
      sb.AppendLine("############### ERRORLOG ###############");
      sb.AppendLine("########################################");

      Dictionary<int, (Diagnostic, int)> diagnosticOccurrences = [];
      foreach (var diag in Diagnostics)
         if (!diagnosticOccurrences.TryAdd(diag.Descriptor.Id, (diag, 1)))
            diagnosticOccurrences[diag.Descriptor.Id] =
               (diagnosticOccurrences[diag.Descriptor.Id].Item1,
                diagnosticOccurrences[diag.Descriptor.Id].Item2 + 1);

      int error,
          warning,
          info;

      error = warning = info = 0;

      foreach (var diag in Diagnostics)
         switch (diag.Severity)
         {
            case DiagnosticSeverity.Error:
               error++;
               break;
            case DiagnosticSeverity.Warning:
               warning++;
               break;
            case DiagnosticSeverity.Information:
               info++;
               break;
            default:
               throw new ArgumentOutOfRangeException();
         }

      sb.AppendLine("");
      sb.AppendLine("");
      sb.AppendLine("# Total Diagnostics: " + Diagnostics.Count);
      sb.AppendLine("# Errors: " + error);
      sb.AppendLine("# Warnings: " + warning);
      sb.AppendLine("# Info: " + info);

      sb.AppendLine("");
      sb.AppendLine("# ID | Name                                    | Occurrences");
      foreach (var kvp in diagnosticOccurrences)
         sb.AppendLine($"#{kvp.Key,4}: {kvp.Value.Item1.Code,-40}: {kvp.Value.Item2} occurrences");
      sb.AppendLine("");

      IndentedStringBuilder.Merge(sb, ExportToConsole());
   }

   private static StringBuilder ExportToConsole()
   {
      Dictionary<string, int> diagnosticsPerFile = [];
      foreach (var diag in Diagnostics)
      {
         var sanitizedPath = FileManager.SanitizePath(diag.Context.FileObj.Path.FullPath, '\\');
         foreach (var fileDescriptor in DescriptorDefinitions.FileDescriptors)
         {
            if (!sanitizedPath.StartsWith(fileDescriptor.FilePath))
               continue;

            if (!diagnosticsPerFile.TryAdd(fileDescriptor.FilePath, 1))
               diagnosticsPerFile[fileDescriptor.FilePath]++;
         }
      }

      var sb = new StringBuilder();
      sb.AppendLine("# Arcanum Exported Diagnostics");
      sb.AppendLine();
      sb.AppendLine($"# Exported on: {DateTime.Now}");
      sb.AppendLine($"# Total Diagnostics: {Diagnostics.Count}");
      sb.AppendLine();
      // Parsed files and folders
      sb.AppendLine("# Parsed Files and Folders:");
      var fileDescriptors = DescriptorDefinitions.FileDescriptors.OrderBy(x => x.FilePath).ToList();
      var maxErrorsInFileIntLength = diagnosticsPerFile.Values.Count == 0
                                        ? 1
                                        : diagnosticsPerFile.Values.Max().ToString().Length;
      foreach (var descriptor in fileDescriptors)
      {
         var sanitizedPath = FileManager.SanitizePath(descriptor.FilePath);
         diagnosticsPerFile.TryGetValue(sanitizedPath, out var errorCount);
         sb.AppendLine($"- ({errorCount.ToString().PadLeft(maxErrorsInFileIntLength)}) | {sanitizedPath}");
      }

      sb.AppendLine();
      sb.AppendLine("Format:");
      sb.AppendLine("# Error Type: Error Name, ID: Error ID, Occurrences: Count, Severity: Severity");
      sb.AppendLine("--> Description: Error Description (replace the {x} with the 0 indexed argument from the line below to get the full error message)");
      sb.AppendLine("- FilePath (Line Number, Column Number) || Argument1 -|- Argument2 -|- ...");
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine();

      var diagnosticsByType = Diagnostics
                             .GroupBy(d => d.Descriptor.Name)
                             .ToDictionary(g => g.Key, g => g.ToList());

      foreach (var kvp in diagnosticsByType)
      {
         sb.AppendLine($"# Error Type: {kvp.Key}, ID: {kvp.Value.First().Descriptor.Id}, Occurrences: {kvp.Value.Count}, Severity: {kvp.Value.First().Severity}");
         sb.AppendLine($"--> Description: {kvp.Value.First().Descriptor.Description.Replace("\n", "\n    ")}");
         sb.AppendLine();
         foreach (var diagnostic in kvp.Value)
            sb.AppendLine($"- {FileManager.SanitizePath(diagnostic.Context.FileObj.Path.FullPath)} (Line {diagnostic.Context.LineNumber}, Column {diagnostic.Context.ColumnNumber}) || {string.Join(" -|- ", diagnostic.Arguments)}");
         sb.AppendLine();
      }

      return sb;
   }

   public static void ExportToFile()
   {
      var sb = ExportToConsole();
      var folder = string.IsNullOrWhiteSpace(Config.Settings.ErrorLogOptions.ExportFilePath)
                      ? IO.IO.GetArcanumDataPath
                      : Config.Settings.ErrorLogOptions.ExportFilePath;
      var filePath = Path.Combine(folder, "ExportedDiagnosticsPdx.txt");

      IO.IO.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

      if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
         ProcessHelper.OpenFile(filePath);
   }
}