using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.API.Attributes;
using Arcanum.Core.Utils.DelayedEvents;
using Arcanum.UI.Components.StyleClasses;
using Arcanum.UI.Components.Windows.MinorWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class PropertyGrid
{
   // Keep track of all open property grids to avoid opening multiple windows for the same object
   public static List<object> OpenEditorsStack { get; } = [];

   // We only trigger this event after a delay to avoid flooding the UI with events
   public readonly PropGridDelayEvent PropertyValueChanged = new(250);
   public event EventHandler<SelectionChangedEventArgs>? PropertySelected = delegate { };
   public PropertyGrid? InlinedPropertyGrid;

   public bool HasInlinedPropertyGrid => InlinedPropertyGrid != null;

   public static Dictionary<Type, Func<object, string>> CustomTypeConverters { get; } = new()
   {
      [typeof(KeyGesture)] = obj
         => ((KeyGesture)obj).GetDisplayStringForCulture(CultureInfo.CurrentCulture) ?? string.Empty,
   };

   public PropertyGrid()
   {
      InitializeComponent();
      Properties = [];
      BorderThickness = new(2);
      Margin = new(2);
      PropertyList.SelectionChanged += OnPropertySelected;
      PropertyList.SelectionChanged += OnPropertyListOnSelectionChanged;
   }

   public static bool CanOpenEditorFor(object obj)
   {
      if (obj == null!)
         return false;

      var type = obj.GetType();
      if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
         return false;

      if (typeof(ICollection).IsAssignableFrom(type))
         return false;

      if (OpenEditorsStack.Contains(obj))
         return false;

      return true;
   }

   public static bool InformIfEditorAvailable(object obj)
   {
      if (CanOpenEditorFor(obj))
         return true;

      MBox.Show("The selected object cannot be edited in a property grid or is already open in another window.",
                "Cannot Open Property Grid",
                MBoxButton.OK,
                MessageBoxImage.Warning);
      return false;
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

   public bool ShowGridEmbedded
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         GridEmbeddedBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
      }
   }

   public bool ForceInlinePropertyGrid { get; set; }

   private void OnPropertyListOnSelectionChanged(object sender, SelectionChangedEventArgs _)
   {
      if (sender is not ListBox { SelectedItem: PropertyItem item })
      {
         Description = string.Empty;
         return;
      }

      SelectionChangedInternal(item);
   }

   private void SelectionChangedInternal(PropertyItem item)
   {
      SelectedPropertyItem = item;

      if (SelectedObject == null)
         return;

      var prop = SelectedObject.GetType().GetProperty(item.PropertyInfo.Name);
      if (prop == null)
         throw new ArgumentException($"Property {item.PropertyInfo.Name} not found in {SelectedObject.GetType().Name}");

      if (ForceInlinePropertyGrid || prop.GetCustomAttribute<InlinePropertyGrid>() is not null)
      {
         // throw an exception if the selected property is not a class or struct
         if (prop.PropertyType is { IsClass: false, IsValueType: false })
            throw new
               ArgumentException($"Property {item.PropertyInfo.Name} is not a class or struct and thus not valid for inline property grid.");

         InlinedPropertyGrid ??= new() { Margin = new(-1) };
         GridEmbeddedBorder.Child = InlinedPropertyGrid;

         InlinedPropertyGrid.SelectedObject = prop.GetValue(SelectedObject);
         ShowGridEmbedded = true;

         InlinedPropertyGrid.Description = prop.GetCustomAttribute<DescriptionAttribute>()?.Description ??
                                           $"No description for {item.PropertyInfo.Name}";
         Description = string.Empty;
      }
      else
      {
         if (InlinedPropertyGrid != null)
            InlinedPropertyGrid.SelectedObject = null;
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

      var props = e.NewValue.GetType()
                   .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

      foreach (var prop in props)
      {
         if (!prop.CanRead)
            continue;

         if (prop.GetCustomAttribute<IgnoreInPropertyGrid>() is not null)
            continue;

         var categoryAttr = prop.GetCustomAttribute<CategoryAttribute>();
         grid.SetValue(TitleProperty, e.NewValue.GetType().FullName);

         if (prop.GetIndexParameters().Length > 0)
            continue; // skip indexers

         var isStatic = prop.GetMethod?.IsStatic ?? false;

         object Getter() => prop.GetValue(isStatic ? null : e.NewValue)!;

         Action<object>? setter;
         if (prop.CanWrite)
            setter = v =>
            {
               var targetType = prop.PropertyType;
               object? safeValue;
               if (v == null! || targetType.IsInstanceOfType(v))
                  safeValue = v;
               else
                  safeValue = Convert.ChangeType(v,
                                                 targetType,
                                                 CultureInfo.InvariantCulture);

               if (isStatic)
                  prop.SetValue(null, safeValue);
               else
                  prop.SetValue(e.NewValue, safeValue);
            };
         else
            setter = null;

         var newItem = new PropertyItem(prop, prop.PropertyType, Getter, setter, categoryAttr?.Category!);
         newItem.ValueChanged += grid.OnPropertyValueChanged;
         grid.Properties.Add(newItem);
      }
   }

   private void ViewCollection_Button_Click(object sender, RoutedEventArgs e)
   {
      if (sender is not BaseButton { DataContext: PropertyItem item })
         return;

      var collection = item.Value as ICollection;
      if (collection == null)
         return;

      var collectionView = new BaseCollectionView(collection) { WindowStartupLocation = WindowStartupLocation.CenterOwner, };
      collectionView.ShowDialog();
   }

   private void ViewObject_Button_Click(object? sender, RoutedEventArgs e)
   {
      if (sender is not BaseButton { DataContext: PropertyItem item })
         return;

      if (item.Value == null!)
         return;

      if (!InformIfEditorAvailable(item.Value))
         return;

      var objectView =
         new PropertyGridWindow(item.Value) { WindowStartupLocation = WindowStartupLocation.CenterOwner, };
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

   public PropertyGrid GetActive()
   {
      return ShowGridEmbedded ? InlinedPropertyGrid?.GetActive() ?? this : this;
   }

   public bool NavigateToProperty(string propertyName)
   {
      foreach (var item in Properties)
         if (item.PropertyInfo.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
         {
            PropertyList.SelectedItem = item;
            SelectionChangedInternal(item);
            return true;
         }

      return false;
   }

   private void ViewEnumArray_Button_Click(object sender, RoutedEventArgs e)
   {
      if (sender is not BaseButton { DataContext: PropertyItem item })
         return;

      if (item.Value is not Enum[] enumArray)
         return;

      ICollection<string> collection = enumArray.Select(num => num.ToString()).ToList();

      var first = collection.Cast<object>().FirstOrDefault();
      if (first == null)
         return;

      var values = Enum.GetValues(enumArray.GetValue(0)!.GetType());
      var available = values.Cast<object>()
                            .Select(v => v.ToString() ?? string.Empty)
                            .Where(v => !string.IsNullOrEmpty(v))
                            .ToList();

      var collectionEditor = new CollectionEditor(available, collection);
      collectionEditor.ShowDialog();
      var selectedItems = collectionEditor.SelectedItems
                                          .Select(value => Enum.Parse(enumArray.GetValue(0)!.GetType(), value))
                                          .Cast<Enum>()
                                          .ToArray();

      item.Value = selectedItems;
      item.RefreshValue();
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