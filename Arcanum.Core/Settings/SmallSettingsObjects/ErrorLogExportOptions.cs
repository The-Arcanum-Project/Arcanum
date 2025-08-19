using System.ComponentModel;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class ErrorLogExportOptions
{
   [Description("'*,' = Export this column, 'x' = Do not export this column\nThere are 4 columns in the error log: Severity, Message, Action, Description")]
   [DefaultValue("*,*,*,*,")]
   public string ColumnsToExport { get; set; } = "*,*,*,*,";
   
   [Description("The name of the exported file. Default is 'ErrorLogExport.csv'.")]
   [DefaultValue("ErrorLogExport.csv")]
   public string ExportFileName { get; set; } = "ErrorLogExport.csv";
   
   [Description("The path where the exported file will be saved. Default is the ArcanumData directory.")]
   [DefaultValue("")]
   public string ExportFilePath { get; set; } = string.Empty;
}