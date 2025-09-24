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
   public AgsWindow()
   {
      InitializeComponent();

      var types = AgsRegistry.Ags.ToList();
      types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
      AgsTypes = types;
      AgsItems = [];
   }

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

   private void AgsItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Type type })
         return;

      if (type.ImplementsGenericInterface(typeof(IEu5ObjectProvider<>), out var implementedType) &&
          implementedType != null)
      {
         var methodInfo = type.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
         if (methodInfo != null)
         {
            var allItems = (IDictionary)methodInfo.Invoke(null, null)!;
            AgsItems = allItems.Values.Cast<IEu5Object>().ToList();
            AgsItems.Sort((x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
            AgsItemsView.SelectedIndex = 0;
            if (AgsItems.Count > 0)
               SetComplexity(AgsItems[0]);
            return;
         }
      }

      AgsItems = [];
      FormattedText = "";
   }

   private void SetComplexity(IAgs ags)
   {
      var complexity = ags.EstimateObjectComplexity();
      Complexity = $"Complexity: {complexity}";
   }

   private void Eu5FileObjSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Eu5FileObj fileObj })
         return;

      FormatFile(fileObj);
   }

   private void FormatFile(Eu5FileObj fileObj)
   {
      var objs = fileObj.ObjectsInFile;
      var sw = System.Diagnostics.Stopwatch.StartNew();
      string formattedStr;
      if (AllowMultithreadedCheckBox.IsChecked == true)
      {
         formattedStr = SavingUtil.FormatFilesMultithreadedIf(objs.ToList());
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

      if (formattedStr.Length > 10_000)
         formattedStr = formattedStr[..10_000] + "\n... (truncated, too long)";
      FormattedText = formattedStr;
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: IEu5Object ags })
         return;

      FileObjs = ags.Source.Descriptor.Files;

      var context = ags.ToAgsContext();
      var sb = new IndentedStringBuilder();
      var sw = System.Diagnostics.Stopwatch.StartNew();
      context.BuildContext(sb);
      sw.Stop();
      SavingTime = (int)sw.ElapsedMilliseconds;
      var formattedStr = sb.ToString();
      if (formattedStr.Length > 10_000)
         formattedStr = formattedStr[..10_000] + "\n... (truncated, too long)";
      FormattedText = formattedStr;
   }

   private void ExportButton_Click(object sender, RoutedEventArgs e)
   {
   }

   private void CopyButton_Click(object sender, RoutedEventArgs e)
   {
      Clipboard.SetText(FormattedText);
   }

   private void Eu5FileObjSelector_OnMouseDown(object sender, MouseButtonEventArgs e)
   {
      var listView = sender as ListView;

      if (listView?.SelectedItem is not Eu5FileObj clickedItem)
         return;

      FormatFile(clickedItem);
   }
}