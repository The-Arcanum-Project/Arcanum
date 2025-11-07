using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.DevHelper;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class AgsWindow
{
   // Store the full, unfiltered lists
   private List<Type> _allAgsTypes = [];
   private List<IEu5Object> _allAgsItems = [];
   private List<Eu5FileObj> _allFileObjs = [];

   public AgsWindow()
   {
      InitializeComponent();

      _allAgsTypes = AgsRegistry.Ags.ToList();
      _allAgsTypes.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

      FilterAndDisplayTypes(); // Initial population
   }

   #region Filtering Logic

   private void OnTypeSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayTypes(TypeSearchBox.Text);
   }

   private void OnItemSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayItems(ItemSearchBox.Text);
   }

   private void OnFileSearchChanged(object sender, TextChangedEventArgs e)
   {
      FilterAndDisplayFiles(FileSearchBox.Text);
   }

   private void FilterAndDisplayTypes(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allAgsTypes
                        : _allAgsTypes.Where(t => t.Name.Contains(filter, StringComparison.OrdinalIgnoreCase));

      AgsTypes = new(filtered);

      if (AgsTypes.Count == 0)
      {
         // If filter results in no types, clear downstream lists
         _allAgsItems.Clear();
         FilterAndDisplayItems();
      }
   }

   private void FilterAndDisplayItems(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allAgsItems
                        : _allAgsItems.Where(i => i.UniqueId.Contains(filter, StringComparison.OrdinalIgnoreCase));

      AgsItems = new(filtered);
      AgsItems.Sort((x, y) => string.Compare(x.UniqueId, y.UniqueId, StringComparison.Ordinal));

      if (AgsItems.Count == 0)
      {
         // If filter results in no items, clear downstream list
         _allFileObjs.Clear();
         FilterAndDisplayFiles();
      }
   }

   private void FilterAndDisplayFiles(string filter = "")
   {
      var filtered = string.IsNullOrWhiteSpace(filter)
                        ? _allFileObjs
                        : _allFileObjs.Where(f => f.Path.Filename.Contains(filter, StringComparison.OrdinalIgnoreCase));

      FileObjs = new(filtered);
   }

   #endregion

   #region Dependency Properties

   public List<Type> AgsTypes
   {
      get => (List<Type>)GetValue(AgsTypesProperty);
      set => SetValue(AgsTypesProperty, value);
   }

   public static readonly DependencyProperty AgsTypesProperty =
      DependencyProperty.Register(nameof(AgsTypes),
                                  typeof(List<Type>),
                                  typeof(AgsWindow),
                                  new(new List<Type>()));

   public List<IEu5Object> AgsItems
   {
      get => (List<IEu5Object>)GetValue(AgsItemsProperty);
      set => SetValue(AgsItemsProperty, value);
   }

   public static readonly DependencyProperty AgsItemsProperty =
      DependencyProperty.Register(nameof(AgsItems),
                                  typeof(List<IEu5Object>),
                                  typeof(AgsWindow),
                                  new(new List<IEu5Object>()));

   public static readonly DependencyProperty FormattedTextProperty =
      DependencyProperty.Register(nameof(FormattedText), typeof(string), typeof(AgsWindow), new(default(string)));

   public string FormattedText
   {
      get => (string)GetValue(FormattedTextProperty);
      set => SetValue(FormattedTextProperty, value);
   }

   public static readonly DependencyProperty ComplexityProperty =
      DependencyProperty.Register(nameof(Complexity), typeof(string), typeof(AgsWindow), new(default(string)));

   public string Complexity
   {
      get => (string)GetValue(ComplexityProperty);
      set => SetValue(ComplexityProperty, value);
   }

   public static readonly DependencyProperty SavingTimeProperty =
      DependencyProperty.Register(nameof(SavingTime), typeof(int), typeof(AgsWindow), new(0));

   public int SavingTime
   {
      get => (int)GetValue(SavingTimeProperty);
      set => SetValue(SavingTimeProperty, value);
   }

   public static readonly DependencyProperty FileObjsProperty =
      DependencyProperty.Register(nameof(FileObjs),
                                  typeof(List<Eu5FileObj>),
                                  typeof(AgsWindow),
                                  new(new List<Eu5FileObj>()));

   public List<Eu5FileObj> FileObjs
   {
      get => (List<Eu5FileObj>)GetValue(FileObjsProperty);
      set => SetValue(FileObjsProperty, value);
   }

   #endregion

   private void AgsItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      _allAgsItems.Clear(); // Clear previous selection's data

      if (sender is ListView { SelectedItem: Type type })
      {
         if (type.ImplementsGenericInterface(typeof(IEu5ObjectProvider<>), out var implementedType) &&
             implementedType != null)
         {
            var methodInfo = type.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
            if (methodInfo != null)
            {
               var allItems = (IDictionary)methodInfo.Invoke(null, null)!;
               _allAgsItems = allItems.Values.Cast<IEu5Object>().ToList();
            }
         }
      }

      // Filter and display the items (will be empty if no type was selected or if type had no items)
      FilterAndDisplayItems(ItemSearchBox.Text);

      // Automatically select the first item if the list is populated
      if (AgsItems.Count != 0)
      {
         AgsItemsView.SelectedIndex = 0;
      }
   }

   private void SetComplexity(IAgs ags)
   {
      var complexity = ags.EstimateObjectComplexity();
      Complexity = $"Complexity: {complexity}";
   }

   private void Eu5FileObjSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Eu5FileObj fileObj })
      {
         // Clear preview if selection is lost
         FormattedText = string.Empty;
         return;
      }

      FormatFile(fileObj);
   }

   private void FormatFile(Eu5FileObj fileObj)
   {
      var objs = fileObj.ObjectsInFile;
      var sw = System.Diagnostics.Stopwatch.StartNew();
      string formattedStr;
      if (AllowMultithreadedCheckBox.IsChecked == true)
      {
         formattedStr = SavingUtil.FormatFilesMultithreadedIf(objs.ToList()).ToString();
         sw.Stop();
      }
      else
      {
         var sb = new IndentedStringBuilder();
         foreach (var obj in objs)
            obj.ToAgsContext().BuildContext(sb);
         sw.Stop();
         formattedStr = sb.ToString();
      }

      SavingTime = (int)sw.ElapsedMilliseconds;

      FormattedText = formattedStr; // Removed truncation for better debugging
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      _allFileObjs.Clear(); // Clear previous selection's data
      FormattedText = "";
      Complexity = "";

      if (sender is ListView { SelectedItem: IEu5Object ags })
      {
         _allFileObjs = ags.Source.Descriptor.Files;
         SetComplexity(ags);

         var context = ags.ToAgsContext();
         var sb = new IndentedStringBuilder();
         var sw = System.Diagnostics.Stopwatch.StartNew();
         context.BuildContext(sb);
         sw.Stop();
         SavingTime = (int)sw.ElapsedMilliseconds;
         FormattedText = sb.ToString(); // Removed truncation
      }

      FilterAndDisplayFiles(FileSearchBox.Text);
   }

   private void ExportButton_Click(object sender, RoutedEventArgs e)
   {
      // Not implemented
   }

   private void CopyButton_Click(object sender, RoutedEventArgs e)
   {
      Clipboard.SetText(FormattedText);
   }

   private void Eu5FileObjSelector_OnMouseDown(object sender, MouseButtonEventArgs e)
   {
      if (e.ClickCount == 2 && sender is ListView { SelectedItem: Eu5FileObj clickedItem })
         FormatFile(clickedItem);
   }
}