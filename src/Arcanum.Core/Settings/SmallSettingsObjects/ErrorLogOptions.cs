using System.ComponentModel;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class ErrorLogOptions() : InternalSearchableSetting(Config.Settings)
{
   private bool _probeFiles = true;
   private bool _vanillaErrorsCausePopups;
   private bool _suppressAllErrors;
   private string _exportFilePath = string.Empty;
   private string _exportFileName = "ErrorLogExport.csv";
   private string _columnsToExport = "*,*,*,*,";
   [Description("'*,' = Export this column, 'x' = Do not export this column\nThere are 4 columns in the error log: Severity, Message, Action, Description")]
   [DefaultValue("*,*,*,*,")]
   public string ColumnsToExport
   {
      get => _columnsToExport;
      set => SetNotifyProperty(ref _columnsToExport, value);
   }

   [Description("The name of the exported file. Default is 'ErrorLogExport.csv'.")]
   [DefaultValue("ErrorLogExport.csv")]
   public string ExportFileName
   {
      get => _exportFileName;
      set => SetNotifyProperty(ref _exportFileName, value);
   }

   [Description("The path where the exported file will be saved. Default is the ArcanumData directory via `string.Empty`.")]
   [DefaultValue("")]
   public string ExportFilePath
   {
      get => _exportFilePath;
      set => SetNotifyProperty(ref _exportFilePath, value);
   }

   [Description("If enabled, Arcanum will probe files to show the region the error occured in.")]
   [DefaultValue(true)]
   public bool ProbeFiles
   {
      get => _probeFiles;
      set => SetNotifyProperty(ref _probeFiles, value);
   }

   [Description("If true vanilla errors will also cause popups")]
   [DefaultValue(false)]
   public bool VanillaErrorsCausePopups
   {
      get => _vanillaErrorsCausePopups;
      set => SetNotifyProperty(ref _vanillaErrorsCausePopups, value);
   }

   [Description("If true, all error popups will be suppressed.")]
   [DefaultValue(false)]
   public bool SuppressAllErrors
   {
      get => _suppressAllErrors;
      set => SetNotifyProperty(ref _suppressAllErrors, value);
   }
}