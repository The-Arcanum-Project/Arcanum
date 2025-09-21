using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.DevHelper;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class ExportFileWindow : INotifyPropertyChanged
{
   public ObservableCollection<FileObj> AvailableFiles { get; } = [];
   public ObservableCollection<string> ObjectsInFile { get; } = [];
   public ObservableCollection<Type> AvailableTypes { get; }

   private FileObj? _selectedFile;
   private Type? _selectedType;

   private readonly Dictionary<FileObj, List<IEu5Object>> _filesDict = [];
   private string _previewText = string.Empty;

   public string PreviewText
   {
      get => _previewText;
      set
      {
         if (value == _previewText)
            return;

         _previewText = value;
         OnPropertyChanged();
      }
   }

   public ExportFileWindow()
   {
      InitializeComponent();

      AvailableTypes = new(AgsRegistry.Ags);
      if (AvailableTypes.Count > 0)
      {
         _selectedType = AvailableTypes[0];
         LoadAvailableFiles();
      }
   }

   private void OnTypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
   {
      if (e.AddedItems.Count > 0 && e.AddedItems[0] is Type type)
      {
         _selectedType = type;
         LoadAvailableFiles();
      }
   }

   private void OnFileSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
   {
      if (e.AddedItems.Count > 0 && e.AddedItems[0] is FileObj file)
      {
         _selectedFile = file;
         LoadObjectsInFile();
         RenderPreview();
      }
   }

   private void LoadAvailableFiles()
   {
      if (_selectedType == null)
         return;

      if (!_selectedType.ImplementsGenericInterface(typeof(IEu5Object<>), out var eu5Object) || eu5Object == null)
         return;

      IDictionary allItems;

      var methodInfo = _selectedType.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
      if (methodInfo != null)
      {
         allItems = (IDictionary)methodInfo.Invoke(null, null)!;
      }
      else if (_selectedType.ImplementsGenericInterface(typeof(IEu5ObjectProvider<>), out var implementedType) &&
               implementedType != null)
      {
         if (typeof(IEu5Object).IsAssignableFrom(_selectedType))
         {
            var emptyProperty = _selectedType.GetProperty("Empty", BindingFlags.Public | BindingFlags.Static);
            if (emptyProperty != null && emptyProperty.GetValue(null) is IEu5Object emptyInstance)
               allItems = emptyInstance.GetGlobalItemsNonGeneric();
            else
               return;
         }
         else
            return;
      }
      else
      {
         return;
      }

      AvailableFiles.Clear();
      _filesDict.Clear();

      foreach (var item in allItems.Values)
      {
         if (item is not IEu5Object eu5Obj)
            continue;

         if (!_filesDict.TryGetValue(eu5Obj.Source, out var value))
         {
            value = [];
            _filesDict[eu5Obj.Source] = value;
            AvailableFiles.Add(eu5Obj.Source);
         }

         value.Add(eu5Obj);
      }

      AvailableFileObjectsListView.SelectedIndex = 0;
   }

   private void LoadObjectsInFile()
   {
      if (_selectedFile == null || !_filesDict.TryGetValue(_selectedFile, out var value))
         return;

      ObjectsInFile.Clear();
      foreach (var obj in value)
         ObjectsInFile.Add(obj.UniqueId);
   }

   private void RenderPreview()
   {
      if (_selectedFile == null || !_filesDict.TryGetValue(_selectedFile, out var value))
         return;

      var sb = new IndentedStringBuilder();
      foreach (var obj in value)
         obj.ToAgsContext().BuildContext(sb);

      PreviewText = sb.ToString();
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }

   private void OnRefreshButtonClick(object sender, RoutedEventArgs e)
   {
      RenderPreview();
   }
}