using System.Collections;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.UI.SpecializedEditors.EditorControls;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class EntitySelector
{
   public static readonly DependencyProperty AddCommandProperty =
      DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(EntitySelector), new(null));

   public static readonly DependencyProperty RemoveCommandProperty =
      DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(EntitySelector), new(null));

   public static readonly DependencyProperty PlaceholderProperty =
      DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(EntitySelector), new(string.Empty));

   public static readonly DependencyProperty SelectedItemProperty =
      DependencyProperty.Register(nameof(SelectedItem),
                                  typeof(object),
                                  typeof(EntitySelector),
                                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

   public static readonly DependencyProperty AddedItemsProperty =
      DependencyProperty.Register(nameof(AddedItems),
                                  typeof(IEnumerable),
                                  typeof(EntitySelector),
                                  new(null, OnAddedItemsChanged));

   public static readonly DependencyProperty SelectedAddedItemProperty =
      DependencyProperty.Register(nameof(SelectedAddedItem),
                                  typeof(object),
                                  typeof(EntitySelector),
                                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

   public static readonly DependencyProperty GridColumnsProperty =
      DependencyProperty.Register(nameof(GridColumns), typeof(int), typeof(EntitySelector), new(5));

   public static readonly DependencyProperty GridItemTemplateProperty =
      DependencyProperty.Register(nameof(GridItemTemplate), typeof(DataTemplate), typeof(EntitySelector), new(null));

   public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(EntitySelector), new(string.Empty));

   public static readonly DependencyProperty AddCommandParameterProperty =
      DependencyProperty.Register(nameof(AddCommandParameter), typeof(object), typeof(EntitySelector), new(null));

   public static readonly DependencyProperty RemoveCommandParameterProperty =
      DependencyProperty.Register(nameof(RemoveCommandParameter), typeof(object), typeof(EntitySelector), new(null));

   public static readonly DependencyProperty IsUniqueInPropertyProperty =
      DependencyProperty.Register(nameof(IsUniqueInProperty), typeof(bool), typeof(EntitySelector), new(false));

   public static readonly DependencyProperty IsUniqueInAllProperty =
      DependencyProperty.Register(nameof(IsUniqueInAll), typeof(bool), typeof(EntitySelector), new(false));

   public static readonly DependencyProperty FilteredItemsProperty =
      DependencyProperty.Register(nameof(FilteredItems), typeof(IEnumerable), typeof(EntitySelector), new(null));

   public IEnumerable FilteredItems
   {
      get => (IEnumerable)GetValue(FilteredItemsProperty);
      set => SetValue(FilteredItemsProperty, value);
   }

   public EntitySelector()
   {
      InitializeComponent();

      AddCommand = new RelayCommand(() => PoliticalEditor.Instance.AddLocations(this));
      RemoveCommand = new RelayCommand(() => PoliticalEditor.Instance.RemoveLocations(this));
   }

   public bool IsUniqueInAll
   {
      get => (bool)GetValue(IsUniqueInAllProperty);
      set => SetValue(IsUniqueInAllProperty, value);
   }

   public bool IsUniqueInProperty
   {
      get => (bool)GetValue(IsUniqueInPropertyProperty);
      set => SetValue(IsUniqueInPropertyProperty, value);
   }

   public Country.Field TargetProperty { get; set; } = Country.Field.ControlCores;

   public ICommand AddCommand
   {
      get => (ICommand)GetValue(AddCommandProperty);
      set => SetValue(AddCommandProperty, value);
   }

   public ICommand RemoveCommand
   {
      get => (ICommand)GetValue(RemoveCommandProperty);
      set => SetValue(RemoveCommandProperty, value);
   }

   public string Placeholder
   {
      get => (string)GetValue(PlaceholderProperty);
      set => SetValue(PlaceholderProperty, value);
   }

   public object SelectedItem
   {
      get => GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
   }

   public IEnumerable AddedItems
   {
      get => (IEnumerable)GetValue(AddedItemsProperty);
      set => SetValue(AddedItemsProperty, value);
   }

   public object SelectedAddedItem
   {
      get => GetValue(SelectedAddedItemProperty);
      set => SetValue(SelectedAddedItemProperty, value);
   }

   public int GridColumns
   {
      get => (int)GetValue(GridColumnsProperty);
      set => SetValue(GridColumnsProperty, value);
   }

   public DataTemplate GridItemTemplate
   {
      get => (DataTemplate)GetValue(GridItemTemplateProperty);
      set => SetValue(GridItemTemplateProperty, value);
   }

   public string Title
   {
      get => (string)GetValue(TitleProperty);
      set => SetValue(TitleProperty, value);
   }

   public object AddCommandParameter
   {
      get => GetValue(AddCommandParameterProperty);
      set => SetValue(AddCommandParameterProperty, value);
   }

   public object RemoveCommandParameter
   {
      get => GetValue(RemoveCommandParameterProperty);
      set => SetValue(RemoveCommandParameterProperty, value);
   }
   public ICommand LocationHover { get; } = new RelayCommand<object>(obj =>
   {
      if (obj is not CellItem { Value: Location location })
         return;

      SelectionManager.Preview([location]);
   });

   public ICommand LocationUnhover { get; } = new RelayCommand<object>(obj =>
   {
      if (obj is not CellItem { Value: Location location })
         return;

      SelectionManager.UnPreview([location]);
   });

   private static void OnAddedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
   }
}