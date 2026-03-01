using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public enum SearchBarMode
{
   Search,
   Filter,
}

public partial class SearchAndFilterBar
{
   public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register(nameof(ItemsSource),
                                  typeof(IEnumerable),
                                  typeof(SearchAndFilterBar),
                                  new(null, OnItemsSourceChanged));

   public static readonly DependencyProperty FilteredItemsSourceProperty =
      DependencyProperty.Register(nameof(FilteredItemsSource),
                                  typeof(IEnumerable),
                                  typeof(SearchAndFilterBar),
                                  new(null));

   public static readonly DependencyProperty SearchPathProperty =
      DependencyProperty.Register(nameof(SearchPath),
                                  typeof(string),
                                  typeof(SearchAndFilterBar),
                                  new(null, OnFilterCriteriaChanged));

   public static readonly DependencyProperty ModeProperty =
      DependencyProperty.Register(nameof(Mode),
                                  typeof(SearchBarMode),
                                  typeof(SearchAndFilterBar),
                                  new FrameworkPropertyMetadata(SearchBarMode.Search));

   public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register(nameof(Text),
                                  typeof(string),
                                  typeof(SearchAndFilterBar),
                                  new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterCriteriaChanged));

   public static readonly DependencyProperty IsCaseSensitiveProperty =
      DependencyProperty.Register(nameof(IsCaseSensitive),
                                  typeof(bool),
                                  typeof(SearchAndFilterBar),
                                  new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterCriteriaChanged));

   public static readonly DependencyProperty IsWholeWordProperty =
      DependencyProperty.Register(nameof(IsWholeWord),
                                  typeof(bool),
                                  typeof(SearchAndFilterBar),
                                  new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterCriteriaChanged));

   public static readonly DependencyProperty IsRegexProperty =
      DependencyProperty.Register(nameof(IsRegex),
                                  typeof(bool),
                                  typeof(SearchAndFilterBar),
                                  new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilterCriteriaChanged));

   public static readonly DependencyProperty ShowOptionsProperty =
      DependencyProperty.Register(nameof(ShowOptions),
                                  typeof(bool),
                                  typeof(SearchAndFilterBar),
                                  new FrameworkPropertyMetadata(true));

   private StringComparison _activeComparison;

   private Regex? _activeRegex;

   private Type? _cachedItemType;
   private PropertyInfo? _cachedPropertyInfo;
   private bool _useRegexStrategy;

   public SearchAndFilterBar()
   {
      InitializeComponent();
   }

   public bool ShowOptions
   {
      get => (bool)GetValue(ShowOptionsProperty);
      set => SetValue(ShowOptionsProperty, value);
   }

   public IEnumerable ItemsSource
   {
      get => (IEnumerable)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
   }
   public IEnumerable? FilteredItemsSource
   {
      get => (IEnumerable)GetValue(FilteredItemsSourceProperty);
      set => SetValue(FilteredItemsSourceProperty, value);
   }
   public string? SearchPath
   {
      get => (string?)GetValue(SearchPathProperty);
      set => SetValue(SearchPathProperty, value);
   }

   public SearchBarMode Mode
   {
      get => (SearchBarMode)GetValue(ModeProperty);
      set => SetValue(ModeProperty, value);
   }
   public string Text
   {
      get => (string)GetValue(TextProperty);
      set => SetValue(TextProperty, value);
   }
   public bool IsCaseSensitive
   {
      get => (bool)GetValue(IsCaseSensitiveProperty);
      set => SetValue(IsCaseSensitiveProperty, value);
   }
   public bool IsWholeWord
   {
      get => (bool)GetValue(IsWholeWordProperty);
      set => SetValue(IsWholeWordProperty, value);
   }
   public bool IsRegex
   {
      get => (bool)GetValue(IsRegexProperty);
      set => SetValue(IsRegexProperty, value);
   }

   private void RefreshFilter()
   {
      if (ItemsSource == null!)
      {
         FilteredItemsSource = null;
         return;
      }

      // We explicitly create a NEW list so VirtualizingGrid detects the reference change
      FilteredItemsSource = ItemsSource.Cast<object>().Where(FilterPredicate).ToList();
   }

   private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not SearchAndFilterBar control)
         return;

      if (e.OldValue is INotifyCollectionChanged oldCollection)
         oldCollection.CollectionChanged -= control.OnSourceCollectionChanged;

      if (e.NewValue is INotifyCollectionChanged newCollection)
         newCollection.CollectionChanged += control.OnSourceCollectionChanged;

      control.UpdateSearchStrategy();
      control.RefreshFilter();
   }

   private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      RefreshFilter();
   }

   private static void OnFilterCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      if (d is not SearchAndFilterBar control)
         return;

      control.UpdateSearchStrategy();
      if (e.Property == SearchPathProperty)
      {
         control._cachedItemType = null;
         control._cachedPropertyInfo = null;
      }

      control.RefreshFilter();
   }

   private void UpdateSearchStrategy()
   {
      _activeComparison = IsCaseSensitive
                             ? StringComparison.Ordinal
                             : StringComparison.OrdinalIgnoreCase;

      if (string.IsNullOrEmpty(Text))
      {
         _useRegexStrategy = false;
         _activeRegex = null;
         return;
      }

      // If user wants Regex OR Whole Word, we use the Regex engine.
      if (IsRegex || IsWholeWord)
      {
         _useRegexStrategy = true;
         var options = RegexOptions.Compiled | (IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

         try
         {
            var pattern = Text;

            // If it's NOT regex mode but IS WholeWord, we treat Text as literal but wrap in boundaries
            if (!IsRegex && IsWholeWord)
            {
               pattern = Regex.Escape(Text);
               pattern = $@"\b{pattern}\b";
            }
            else if (IsRegex && IsWholeWord)
               pattern = $@"\b{pattern}\b";

            _activeRegex = new(pattern, options);
         }
         catch
         {
            _activeRegex = null;
         }
      }
      else
      {
         _useRegexStrategy = false;
         _activeRegex = null;
      }
   }

   private bool FilterPredicate(object? item)
   {
      if (item is null)
         return false;
      if (string.IsNullOrEmpty(Text))
         return true;

      string? valueToCheck;

      if (string.IsNullOrEmpty(SearchPath))
         valueToCheck = item.ToString();
      else
      {
         var itemType = item.GetType();

         // Check Cache
         if (_cachedItemType != itemType)
         {
            _cachedItemType = itemType;
            _cachedPropertyInfo = itemType.GetProperty(SearchPath);
         }

         // If property not found on this type, null
         if (_cachedPropertyInfo is null)
            return false;

         valueToCheck = _cachedPropertyInfo.GetValue(item)?.ToString();
      }

      if (string.IsNullOrEmpty(valueToCheck))
         return false;

      if (_useRegexStrategy)
         return _activeRegex is not null && _activeRegex.IsMatch(valueToCheck);

      return valueToCheck.Contains(Text, _activeComparison);
   }

   private void OnSearchInputPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
   {
      if (sender is not TextBox textBox)
         return;

      textBox.Clear();
      textBox.CaretIndex = 0;
      e.Handled = true;
   }
}