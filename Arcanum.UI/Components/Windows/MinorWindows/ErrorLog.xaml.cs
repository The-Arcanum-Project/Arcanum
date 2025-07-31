using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ErrorLog : INotifyPropertyChanged
{
   public enum FilterType
   {
      Severity,
      Name,
      Id,
      Message,
      Description,
      ErrorAction,
      Resolution,
   }

   private string _errorName = "Error Name";
   public string ErrorName
   {
      get => _errorName;
      set
      {
         if (_errorName == value)
            return;

         _errorName = value;
         OnPropertyChanged();
      }
   }

   private string _errorMessage = "Error Message";
   public string ErrorMessage
   {
      get => _errorMessage;
      set
      {
         if (_errorMessage == value)
            return;

         _errorMessage = value;
         OnPropertyChanged();
      }
   }

   private string _errorDescription = "Error Description";
   public string ErrorDescription
   {
      get => _errorDescription;
      set
      {
         if (_errorDescription == value)
            return;

         _errorDescription = value;
         OnPropertyChanged();
      }
   }

   private string _errorResolution = string.Empty;
   public string ErrorResolution
   {
      get => _errorResolution;
      set
      {
         if (_errorResolution == value)
            return;

         _errorResolution = value;
         OnPropertyChanged();
      }
   }

   public ErrorLog()
   {
      InitializeComponent();

      FilterComboBox.ItemsSource = Enum.GetValues(typeof(FilterType));
      ErrorLogListView.ItemsSource = new ListCollectionView(ErrorManager.Diagnostics);

      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.DebugError1,
                                       new(),
                                       DiagnosticSeverity.Error,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));

      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.DebugError1,
                                       new(),
                                       DiagnosticSeverity.Warning,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));
      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.DebugError2,
                                       new(),
                                       DiagnosticSeverity.Information,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));
      ErrorManager.Diagnostics.Add(new(MiscellaneousError.Instance.UnknownError,
                                       new(),
                                       DiagnosticSeverity.Error,
                                       "Test Action",
                                       "Test Message",
                                       "Test Description"));
   }

   private void ErrorLogListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (ErrorLogListView.SelectedItem is Diagnostic diagnostic)
      {
         ErrorName = diagnostic.Descriptor.Name;
         ErrorMessage = diagnostic.Descriptor.Message;
         ErrorDescription = diagnostic.Descriptor.Description;
         ErrorResolution = diagnostic.Descriptor.Resolution;
      }
      else
      {
         ErrorName = "Error Name";
         ErrorMessage = "Error Message";
         ErrorDescription = "Error Description";
         ErrorResolution = string.Empty;
      }
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }
}