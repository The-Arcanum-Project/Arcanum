using System.ComponentModel;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

/// <summary>
/// This enum defines the severity of a diagnostic report.
/// It is used to determine how the error should be handled and displayed to the user.
/// </summary>
public enum DiagnosticReportSeverity
{
   /// Logs error but just continues
   [Description("Logs the error but continues without user interaction.")]
   Silent,

   /// A bit more sever then silent. Just notifies the user about the error, but does not require any action.
   /// The user can continue using the app, but the error will be logged.
   [Description("Notifies with a popup, but does not require action.")]
   PopupNotify,

   /// A breaking error. The user will have to decide if he want to close the app,
   /// retry loading the file (after a custom edit by them), or to skip the error and continue.
   [Description("Gives a warning popup with options to skip the error, reload the file, or close the app.")]
   PopupWarning,

   /// Same as PopupWarning, but the user can only close the app or retry loading the file.
   /// Skipping is not supported due to the severity of the error.
   [Description("Gives a error popup with options to reload the file or close the app.")]
   PopupError,

   /// Do not log the error
   [Description("Completely ignores this type of diagnostic and does not log it.")]
   Suppressed,
}