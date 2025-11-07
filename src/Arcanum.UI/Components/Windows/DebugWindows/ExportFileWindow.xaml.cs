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
// Add these using statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class ExportFileWindow : INotifyPropertyChanged
{
   public ObservableCollection<Type> AvailableTypes { get; } = [];
   public ObservableCollection<Eu5FileObj> AvailableFiles { get; } = [];
   public ObservableCollection<string> ObjectsInFile { get; } = [];

   private Eu5FileObj? _selectedFile;
   private Type? _selectedType;

   // Store the full, unfiltered lists
   private List<Type> _allTypes = [];
   private List<Eu5FileObj> _allAvailableFiles = [];
   private List<string> _allObjectsInFile = [];

   private readonly Dictionary<Eu5FileObj, List<IEu5Object>> _filesDict = [];
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

      _allTypes = new List<Type>(AgsRegistry.Ags);
      FilterAndDisplayTypes(); // Initial population

      if (AvailableTypes.Count > 0)
      {
         // Select the first type by default, which will trigger the subsequent loads
         TypesListView.SelectedIndex = 0;
      }
   }

   #region Filtering Logic

   private void OnTypeSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayTypes(TypeSearchBox.Text);
   }

   private void OnFileSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayFiles(FileSearchBox.Text);
   }

   private void OnObjectInFileSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayObjectsInFile(ObjectInFileSearchBox.Text);
   }

   private void FilterAndDisplayTypes(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allTypes
                        : _allTypes.Where(t => t.Name.Contains(filter, StringComparison.OrdinalIgnoreCase));

      AvailableTypes.Clear();
      foreach (var type in filtered.OrderBy(t => t.Name))
      {
         AvailableTypes.Add(type);
      }
   }

   private void FilterAndDisplayFiles(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allAvailableFiles
                        : _allAvailableFiles.Where(f => f.Path.Filename.Contains(filter,
                                                       StringComparison.OrdinalIgnoreCase));

      AvailableFiles.Clear();
      foreach (var file in filtered.OrderBy(f => f.Path.Filename))
      {
         AvailableFiles.Add(file);
      }
   }

   private void FilterAndDisplayObjectsInFile(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allObjectsInFile
                        : _allObjectsInFile.Where(id => id.Contains(filter, StringComparison.OrdinalIgnoreCase));

      ObjectsInFile.Clear();
      foreach (var objId in filtered.OrderBy(id => id))
      {
         ObjectsInFile.Add(objId);
      }
   }

   #endregion

   private void OnTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (e.AddedItems.Count > 0 && e.AddedItems[0] is Type type)
      {
         _selectedType = type;
         LoadAvailableFiles();
      }
   }

   private void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      // Clear previous selection's data
      ObjectsInFile.Clear();
      PreviewText = string.Empty;

      if (e.AddedItems.Count > 0 && e.AddedItems[0] is Eu5FileObj file)
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

      // Clear old data
      _filesDict.Clear();

      foreach (var item in allItems.Values)
      {
         if (item is not IEu5Object eu5Obj)
            continue;

         if (!_filesDict.TryGetValue(eu5Obj.Source, out var value))
         {
            value = [];
            _filesDict[eu5Obj.Source] = value;
         }

         value.Add(eu5Obj);
      }

      // Populate the full list and then apply the current filter
      _allAvailableFiles = new List<Eu5FileObj>(_filesDict.Keys);
      FilterAndDisplayFiles(FileSearchBox.Text);

      if (AvailableFiles.Any())
      {
         AvailableFileObjectsListView.SelectedIndex = 0;
      }
      else
      {
         // If no files match the filter, clear the subsequent lists
         _selectedFile = null;
         _allObjectsInFile.Clear();
         FilterAndDisplayObjectsInFile();
         PreviewText = string.Empty;
      }
   }

   private void LoadObjectsInFile()
   {
      if (_selectedFile == null || !_filesDict.TryGetValue(_selectedFile, out var value))
      {
         _allObjectsInFile.Clear();
         FilterAndDisplayObjectsInFile(); // Ensure list is cleared
         return;
      }

      // Populate the full list and then apply the current filter
      _allObjectsInFile = value.Select(obj => obj.UniqueId).ToList();
      FilterAndDisplayObjectsInFile(ObjectInFileSearchBox.Text);
   }

   private void RenderPreview()
   {
      if (_selectedFile == null || !_filesDict.TryGetValue(_selectedFile, out var value))
      {
         PreviewText = string.Empty;
         return;
      }

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