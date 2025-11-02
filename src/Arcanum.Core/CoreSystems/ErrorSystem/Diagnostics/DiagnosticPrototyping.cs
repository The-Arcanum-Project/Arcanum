// using System.Diagnostics;
// using Arcanum.Nexus.Core.CoreSystems.Common;
// using ErrorProvider = Arcanum.Nexus.Core.CoreSystems.ErrorSystem.BaseErrorTypes.ErrorProvider;
//
// namespace Arcanum.Nexus.Core.CoreSystems.ErrorSystem.Diagnostics;
//
// public static class DiagnosticPrototyping
// {
//    public static bool ConvertToInt(string toConvert, out int value)
//    {
//       if (int.TryParse(toConvert, out value))
//          return true;
//
//       throw new DiagnosticException(ErrorProvider.ConversionED,
//                                     toConvert,
//                                     typeof(int));
//    }
//
//    public static void IgnoreError()
//    {
//    }
//
//    public static void HandleTheError(LocationContext context)
//    {
//             
//       var strings = new List<string>
//       {
//          "a",
//          "b",
//          "c",
//          "d",
//          "e"
//       };
//
//       foreach (var str in strings)
//          try
//          {
//             // Attempt to convert each string to an integer
//             if (ConvertToInt(str, out var value))
//                Debug.WriteLine($"Converted '{str}' to {value}");
//          }
//          catch (DiagnosticException ex)
//          {
//             ex.HandleDiagnostic(context, "Doing weird stuff", reportSeverity: DiagnosticReportSeverity.PopupNotify);
//             // Default case: The Error is ignored
//          }
//    }
//
//    public static void ParseFileExample()
//    {
//       while (true)
//       {
//          
//          var context = new LocationContext
//          {
//             FilePath = "example.txt",
//             LineNumber = 1,
//             ColumnNumber = 1,
//          };
//          try
//          {
//             // Simulate file parsing logic
//             // This could be any operation that might throw a DiagnosticException
//             HandleTheError(context);
//             break; // Exit the loop if parsing is successful
//          }
//          catch (ReloadFileException)
//          {
//          }
//          finally
//          {
//             // Clean up already initialized resources or reset context
//             // Reset state to before the parsing attempt
//          }
//
//          break;
//       }
//    }
//
//    // What options does a function have which calls a subfunction which fails:
//    // Handle the error using the correct DiagnosticReportSeverity
//    // - If it is a silent error, continue to next iteration / step
//    // - If it is a popup error, show the error to the use
//    //   - However if the user might ask for a retry, then the file needs to be read in
//    //   - If it is a skip handle as the silent error
//    //   - If it is a stop, then stop the program
//    // Ignore the error
//    // Escalate the error, since it does not know how to handle it (probably not needed)
//    // So all in all we have three different options:
//    // - Skip to next
//    //   - Only valid in loop
//    // - Reload the error (escalate to the top and redo the file load)
//    // - Stop the program
// }

