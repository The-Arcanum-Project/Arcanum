using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.Utils.DelayedEvents;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.PopUp;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class PropertyGrid
{
   // We only trigger this event after a delay to avoid flooding the UI with events
   public readonly PropGridDelayEvent PropertyValueChanged = new(250);
   public event EventHandler<SelectionChangedEventArgs>? PropertySelected = delegate { };

   public PropertyGrid()
   {
      InitializeComponent();
      Properties = [];
      BorderThickness = new(2);
      Margin = new(2);
      PropertyList.SelectionChanged += OnPropertySelected;
      PropertyList.SelectionChanged += OnPropertyListOnSelectionChanged;
   }

   public PropertyItem? SelectedPropertyItem
   {
      get => (PropertyItem)PropertyList.SelectedItem;
      set
      {
         if (value == null!)
            return;

         PropertyList.SelectedItem = value;
         Description = value.CollectionDescription;
      }
   }

   public void UpdatePropertyItem()
   {
      PropertyList.Items.Refresh();
   }

   private void OnPropertyListOnSelectionChanged(object sender, SelectionChangedEventArgs _)
   {
      if (sender is not ListBox { SelectedItem: PropertyItem item })
      {
         Description = string.Empty;
         return;
      }

      if (SelectedObject == null)
         return;

      var prop = SelectedObject.GetType().GetProperty(item.PropertyInfo.Name);
      var attr = prop?.GetCustomAttribute<DescriptionAttribute>();
      Description = attr?.Description ?? $"No description for {item.PropertyInfo.Name}";
   }

   public double LabelWidth { get; set; } = 150;

   public static readonly DependencyProperty SelectedObjectProperty =
      DependencyProperty.Register(nameof(SelectedObject),
                                  typeof(object),
                                  typeof(PropertyGrid),
                                  new(null, OnSelectedObjectChanged));

   public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title),
                                  typeof(string),
                                  typeof(PropertyGrid),
                                  new("Property-Grid"));

   public static readonly DependencyProperty DescriptionProperty =
      DependencyProperty.Register(nameof(Description),
                                  typeof(string),
                                  typeof(PropertyGrid),
                                  new(""));

   public string Description
   {
      get => (string)GetValue(DescriptionProperty);
      set => SetValue(DescriptionProperty, value);
   }

   public object? SelectedObject
   {
      get => GetValue(SelectedObjectProperty);
      set => SetValue(SelectedObjectProperty, value);
   }

   // Returns the ToString representation of the selected object
   public string Title
   {
      get => (string)GetValue(TitleProperty);
      set => SetValue(TitleProperty, value);
   }

   public ObservableCollection<PropertyItem> Properties { get; }

   private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not PropertyGrid grid)
         return;

      foreach (var oldItem in grid.Properties)
         oldItem.ValueChanged -= grid.OnPropertyValueChanged;
      grid.Properties.Clear();

      if (e.NewValue == null)
         return;

      var props = e.NewValue.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (var prop in props)
      {
         if (!prop.CanRead)
            continue;

         var categoryAttr = prop.GetCustomAttribute<CategoryAttribute>();
         grid.SetValue(TitleProperty, e.NewValue.GetType().Name);
         var target = e.NewValue;
         Action<object>? setter = prop.CanWrite
                                     ? v =>
                                     {
                                        var targetType = prop.PropertyType;
                                        var safeValue = v == null! || targetType.IsInstanceOfType(v)
                                                           ? v
                                                           : Convert.ChangeType(v,
                                                                                targetType,
                                                                                CultureInfo.InvariantCulture);
                                        prop.SetValue(target, safeValue);
                                     }
                                     : null;

         PropertyItem newItem = new(prop, prop.PropertyType, Getter, setter, categoryAttr?.Category!);
         newItem.ValueChanged += grid.OnPropertyValueChanged;
         grid.Properties.Add(newItem);
         continue;

         object Getter() => prop.GetValue(target)!;
      }
   }

   private void ViewCollection_Button_Click(object sender, RoutedEventArgs e)
   {
      if (sender is not BaseButton { DataContext: PropertyItem item })
         return;

      var collection = item.Value as ICollection;
      if (collection == null)
         return;

      var collectionView = new BaseCollectionView(collection);
      collectionView.ShowDialog();
   }

   private void ViewObject_Button_Click(object? sender, RoutedEventArgs e)
   {
      if (sender is not BaseButton { DataContext: PropertyItem item })
         return;

      if (item.Value == null!)
         return;

      var objectView = new PropertyGridWindow(item.Value);
      objectView.ShowDialog();
   }

   public virtual void OnPropertyValueChanged(PropertyItem propertyItem, object? o)
   {
      PropertyValueChanged.Invoke(this, new(propertyItem, o));
   }

   protected virtual void OnPropertySelected(object sender, SelectionChangedEventArgs e)
   {
      PropertySelected?.Invoke(this, e);
   }
   
   public static AllOptionsTestObject GetAllOptionsTestObject()
   {
      return new ();
   }
}

public class AllOptionsTestObject
{
   public string TestString { get; set; } = "Test String";
   public int TestInt { get; set; } = 42;
   public bool TestBool { get; set; } = true;
   public double TestDouble { get; set; } = 3.14;
   // An enum defined as a flag
   public Key FlagEnum { get; set; } = Key.A;
   // an enum defined as a normal enum
   public Orientation NormalEnum { get; set; } = Orientation.Horizontal;
   public List<string> TestList { get; set; } = ["Item1", "Item2", "Item3"];
}