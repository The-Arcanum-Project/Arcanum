using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.Core.Utils.DevHelper;
using Arcanum.UI.NUI.Generator;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class NUIObjectView
{
   public List<Type> NUIObjectTypes
   {
      get => (List<Type>)GetValue(NUIObjectTypesProperty);
      set => SetValue(NUIObjectTypesProperty, value);
   }

   public static readonly DependencyProperty NUIObjectTypesProperty =
      DependencyProperty.Register(nameof(NUIObjectTypes),
                                  typeof(List<Type>),
                                  typeof(NUIObjectView),
                                  new(new List<Type>()));

   public string SelectedObjectName
   {
      get => (string)GetValue(SelectedObjectNameProperty);
      set => SetValue(SelectedObjectNameProperty, value);
   }

   public static readonly DependencyProperty SelectedObjectNameProperty =
      DependencyProperty.Register(nameof(SelectedObjectName),
                                  typeof(string),
                                  typeof(NUIObjectView),
                                  new(default(string)));

   public List<INUI> NUIObjects
   {
      get => (List<INUI>)GetValue(NUIObjectsProperty);
      set => SetValue(NUIObjectsProperty, value);
   }

   public static readonly DependencyProperty NUIObjectsProperty =
      DependencyProperty.Register(nameof(NUIObjects),
                                  typeof(List<INUI>),
                                  typeof(NUIObjectView),
                                  new(default(List<INUI>)));

   public NUIObjectView()
   {
      InitializeComponent();

      var types = NUITypeRegistry.NUIType.ToList();
      types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
      NUIObjectTypes = types;
   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Type type })
         return;

      if (type.ImplementsGenericInterface(typeof(ICollectionProvider<>), out var implementedType) &&
          implementedType != null)
      {
         var methodInfo = type.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
         if (methodInfo != null)
            NUIObjects = ((IDictionary)methodInfo.Invoke(null, null)!).Values.Cast<INUI>().ToList();
      }
      else if (type.ImplementsGenericInterface(typeof(IEu5ObjectProvider<>), out implementedType) &&
               implementedType != null)
      {
         if (typeof(IEu5Object).IsAssignableFrom(type))
         {
            var emptyProperty = type.GetProperty("Empty", BindingFlags.Public | BindingFlags.Static);
            if (emptyProperty != null && emptyProperty.GetValue(null) is IEu5Object emptyInstance)
               NUIObjects = emptyInstance.GetGlobalItemsNonGeneric().Values.Cast<INUI>().ToList();
         }
      }
      else
      {
         SelectedObjectName = "No objects found";
         NUIObjects = [];
         ViewPresenter.Content = null;
      }

      NUIObjects.Sort((x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
      ObjectListView.SelectedIndex = 0;
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView lv)
         return;

      var selectedItems = lv.SelectedItems.Cast<INUI>().ToList();
      if (selectedItems.Count == 0)
      {
         SelectedObjectName = "No object selected";
         ViewPresenter.Content = null;
         return;
      }

      if (selectedItems.Count == 1)
         SelectedObjectName = selectedItems[0].ToString() ?? selectedItems[0].GetType().Name;
      else
         SelectedObjectName = $"{selectedItems.Count} items selected";

      NUIViewGenerator.GenerateAndSetView(new(selectedItems, true, ViewPresenter));
   }
}