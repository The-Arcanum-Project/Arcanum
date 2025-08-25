using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.DebugTools;
using Arcanum.Core.Utils.Debug;
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

      NUIObjectTypes = NUITypeRegistry.GetAllNUITypes().ToList();

   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Type type })
         return;

      if (type.ImplementsGenericInterface(typeof(ICollectionProvider<>), out var implementedType) &&
          implementedType != null)
      {
         var methodInfo = type.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
         if (methodInfo == null)
            return;

         var allItems = (IEnumerable)methodInfo.Invoke(null, null)!;
         NUIObjects = allItems.Cast<INUI>().ToList();
      }
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: INUI nuiObject })
         return;

      SelectedObjectName = nuiObject.ToString() ?? nuiObject.GetType().Name;
      
      NUIViewGenerator.GenerateAndSetView(new (nuiObject, true, ViewPresenter));
   }
}