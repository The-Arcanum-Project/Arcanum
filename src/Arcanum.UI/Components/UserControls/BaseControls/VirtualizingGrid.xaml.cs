using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class VirtualizingGrid
{
   public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(VirtualizingGrid), new(null, OnGridPropertyChanged));

   public static readonly DependencyProperty ColumnsProperty =
      DependencyProperty.Register(nameof(Columns), typeof(int), typeof(VirtualizingGrid), new(3, OnGridPropertyChanged));

   public static readonly DependencyProperty SelectedItemProperty =
      DependencyProperty.Register(nameof(SelectedItem),
                                  typeof(object),
                                  typeof(VirtualizingGrid),
                                  new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

   public static readonly DependencyProperty ItemTemplateProperty =
      DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(VirtualizingGrid), new(null));

   public static readonly DependencyProperty ChunkedItemsProperty =
      DependencyProperty.Register(nameof(ChunkedItems), typeof(IEnumerable<CellItem[]>), typeof(VirtualizingGrid), new(null));

   private List<CellItem> _allCells = [];

   public VirtualizingGrid() => InitializeComponent();

   public IEnumerable ItemsSource
   {
      get => (IEnumerable)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
   }

   public int Columns
   {
      get => (int)GetValue(ColumnsProperty);
      set => SetValue(ColumnsProperty, value);
   }

   public object SelectedItem
   {
      get => GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
   }

   public DataTemplate ItemTemplate
   {
      get => (DataTemplate)GetValue(ItemTemplateProperty);
      set => SetValue(ItemTemplateProperty, value);
   }

   public IEnumerable<CellItem[]> ChunkedItems
   {
      get => (IEnumerable<CellItem[]>)GetValue(ChunkedItemsProperty);
      private set => SetValue(ChunkedItemsProperty, value);
   }

   private static void OnGridPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is VirtualizingGrid control)
         control.RebuildChunks();
   }

   private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not VirtualizingGrid control)
         return;

      foreach (var cell in control._allCells)
         cell.IsSelected = cell.Value == e.NewValue;
   }

   private void RebuildChunks()
   {
      if (ItemsSource is not { } items || Columns <= 0)
      {
         _allCells = [];
         ChunkedItems = [];
         return;
      }

      _allCells = items.Cast<object>()
                       .Select(x => new CellItem(x) { IsSelected = x == SelectedItem })
                       .ToList();

      ChunkedItems = [.. _allCells.Chunk(Columns)];
   }

   private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
   {
      if (sender is FrameworkElement { DataContext: CellItem cell })
         SelectedItem = cell.Value;
   }
}

public class CellItem(object value) : INotifyPropertyChanged
{
   public object Value { get; } = value;

   public bool IsSelected
   {
      get;
      set
      {
         if (field == value)
            return;

         field = value;
         PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
      }
   }

   public event PropertyChangedEventHandler? PropertyChanged;
}