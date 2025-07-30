using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Arcanum.API.Attributes;
using Arcanum.Core.Utils.DelayedEvents;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.PopUp;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class PropertyGrid
{
   // We only trigger this event after a delay to avoid flooding the UI with events
   public readonly PropGridDelayEvent PropertyValueChanged = new(250);
   public event EventHandler<SelectionChangedEventArgs>? PropertySelected = delegate { };

   private PropertyGrid? _inlinedPropertyGrid = null;

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

   private bool _showGridEmbedded;
   public bool ShowGridEmbedded
   {
      get => _showGridEmbedded;
      set
      {
         if (_showGridEmbedded == value)
            return;

         _showGridEmbedded = value;
         GridEmbeddedBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
      }
   }

   private void OnPropertyListOnSelectionChanged(object sender, SelectionChangedEventArgs _)
   {
      if (sender is not ListBox { SelectedItem: PropertyItem item })
      {
         Description = string.Empty;
         return;
      }

      SelectedPropertyItem = item;

      if (SelectedObject == null)
         return;

      var prop = SelectedObject.GetType().GetProperty(item.PropertyInfo.Name);
      if (prop == null)
         throw new ArgumentException($"Property {item.PropertyInfo.Name} not found in {SelectedObject.GetType().Name}");

      if (prop.GetCustomAttribute<InlinePropertyGrid>() is not null)
      {
         // throw an exception if the selected property is not a class or struct
         if (prop.PropertyType is { IsClass: false, IsValueType: false })
            throw new
               ArgumentException($"Property {item.PropertyInfo.Name} is not a class or struct and thus not valid for inline property grid.");

         _inlinedPropertyGrid ??= new() { Margin = new(1) };
         GridEmbeddedBorder.Child = _inlinedPropertyGrid;

         _inlinedPropertyGrid.SelectedObject = prop.GetValue(SelectedObject);
         ShowGridEmbedded = true;

         _inlinedPropertyGrid.Description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                                            $"No description for {item.PropertyInfo.Name}";
         Description = string.Empty;
      }
      else
      {
         if (_inlinedPropertyGrid != null)
            _inlinedPropertyGrid.SelectedObject = null;
         ShowGridEmbedded = false;
         Description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                       $"No description for {item.PropertyInfo.Name}";
      }

      DescriptionGrid.Visibility = string.IsNullOrEmpty(Description) ? Visibility.Collapsed : Visibility.Visible;
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

         if (prop.GetCustomAttribute<IgnoreInPropertyGrid>() is not null)
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

      var collectionView = new BaseCollectionView(collection)
      {
         WindowStartupLocation = WindowStartupLocation.CenterOwner,
      };
      collectionView.ShowDialog();
   }

   private void ViewObject_Button_Click(object? sender, RoutedEventArgs e)
   {
      if (sender is not BaseButton { DataContext: PropertyItem item })
         return;

      if (item.Value == null!)
         return;

      var objectView = new PropertyGridWindow(item.Value)
      {
         WindowStartupLocation = WindowStartupLocation.CenterOwner,
      };
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
      return new();
   }

   public PropertyGrid GetActive()
   {
      return ShowGridEmbedded ? _inlinedPropertyGrid?.GetActive() ?? this : this;
   }
}

#if DEBUG
public class AllOptionsTestObject
{
   public string TestString { get; set; } = "Test String";
   public int TestInt { get; set; } = 42;
   public bool TestBool { get; set; } = true;
   [DefaultValue(3.14)]
   public double TestDouble { get; set; } = 3.14;
   // An enum defined as a flag
   public Key FlagEnum { get; set; } = Key.A;
   // an enum defined as a normal enum
   public Orientation NormalEnum { get; set; } = Orientation.Horizontal;
   public List<string> TestList { get; set; } = ["Item1", "Item2", "Item3"];

   [InlinePropertyGrid]
   public OuterObject InlineTestObject { get; set; } = new();
}

public class OuterObject
{
   public string Name { get; set; } = "Other Object";
   public int Value { get; set; } = 100;
   [InlinePropertyGrid]
   public InlineTestObject InlineTestObject { get; set; } = new();
}

public class InlineTestObject
{
   public Point Point { get; set; } = new(0, 0);
   public Size Size { get; set; } = new(100, 100);
   public string Name { get; set; } = "Inline Test Object";
}
#endif