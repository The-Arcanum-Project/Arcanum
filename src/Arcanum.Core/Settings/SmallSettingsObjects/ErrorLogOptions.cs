using System.ComponentModel;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class ErrorLogOptions() : InternalSearchableSetting(Config.Settings)
{
   [Description("'*,' = Export this column, 'x' = Do not export this column\nThere are 4 columns in the error log: Severity, Message, Action, Description")]
   [DefaultValue("*,*,*,*,")]
   public string ColumnsToExport
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = "*,*,*,*,";

   [Description("The name of the exported file. Default is 'ErrorLogExport.csv'.")]
   [DefaultValue("ErrorLogExport.csv")]
   public string ExportFileName
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = "ErrorLogExport.csv";

   [Description("The path where the exported file will be saved. Default is the ArcanumData directory via `string.Empty`.")]
   [DefaultValue("")]
   public string ExportFilePath
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = string.Empty;

   [Description("If enabled, Arcanum will probe files to show the region the error occured in.")]
   [DefaultValue(true)]
   public bool ProbeFiles
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If true vanilla errors will also cause popups")]
   [DefaultValue(false)]
   public bool VanillaErrorsCausePopups
   {
      get;
      set => SetNotifyProperty(ref field, value);
   }

   [Description("If true, all error popups will be suppressed.")]
   [DefaultValue(true)]
   public bool SuppressAllErrors
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If true, only vanilla errors will be suppressed.")]
   [DefaultValue(true)]
   public bool SuppressVanillaErrors
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("If true, the error log will always be exported to a file on exit.")]
   [DefaultValue(true)]
   public bool AlwaysExportLogToFile
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = true;

   [Description("The name of the error log file.")]
   [DefaultValue("Arcanum_ErrorLog.log")]
   public string ErrorLogFileName
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = "Arcanum_ErrorLog.log";
}