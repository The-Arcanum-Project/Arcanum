using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static System.Linq.Expressions.Expression;

namespace Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox
{
   /// <summary>
   /// AutoCompleteComboBox.xaml
   /// </summary>
   public class AutoCompleteComboBox : ComboBox
   {
      // Use a CancellationTokenSource to cancel previous search tasks
      private CancellationTokenSource _filterCancellationTokenSource = new();
      private readonly SerialDisposable _disposable = new(); // For Debouncing

      private bool _isUpdatingText;
      private bool _isInitialized;

      private TextBox _editableTextBoxCache = null!;
      private Func<object, string>? _textFromItemDelegate;
      private string? _cachedTextSearchTextPath;
      private string? _cachedDisplayMemberPath;
      private Type? _itemType;

      private IEnumerable? _fullItemsSource;

      public TextBox EditableTextBox
      {
         get
         {
            if (_editableTextBoxCache == null!)
            {
               const string name = "PART_EditableTextBox";
               _editableTextBoxCache = (TextBox)VisualTreeModule.FindChild(this, name)!;
            }

            return _editableTextBoxCache;
         }
      }

      #region IsDropdownOnly DependencyProperty

      /// <summary>
      /// Gets or sets a value indicating whether the ComboBox should behave as a simple dropdown
      /// without any text filtering.
      /// </summary>
      public bool IsDropdownOnly
      {
         get => (bool)GetValue(IsDropdownOnlyProperty);
         set
         {
            SetValue(IsDropdownOnlyProperty, value);
            IsReadOnly = value;
         }
      }

      public static readonly DependencyProperty IsDropdownOnlyProperty =
         DependencyProperty.Register(nameof(IsDropdownOnly),
                                     typeof(bool),
                                     typeof(AutoCompleteComboBox),
                                     new(false, OnIsDropdownOnlyChanged));

      private static void OnIsDropdownOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var comboBox = (AutoCompleteComboBox)d;
         var isDropdownOnly = (bool)e.NewValue;

         comboBox.IsReadOnly = isDropdownOnly;
         if (isDropdownOnly)
            comboBox.ItemsSource = comboBox.FullItemsSource;
         else
            // Re-filter based on current text when switching back to autocomplete mode.
            comboBox.ApplyFilter(comboBox.Text);
      }

      #endregion

      public override void OnApplyTemplate()
      {
         if (_editableTextBoxCache != null!)
            _editableTextBoxCache.KeyDown -= OnEditableTextBoxKeyDown;

         base.OnApplyTemplate();
         _editableTextBoxCache =
            GetTemplateChild("PART_EditableTextBox") as TextBox ?? throw new InvalidOperationException();

         _editableTextBoxCache.KeyDown += OnEditableTextBoxKeyDown;

         if (!IsDropdownOnly)
            ApplyFilter(Text);
         
         _isInitialized = true; 
      }

      private void OnEditableTextBoxKeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.Enter)
         {
            if (IsDropDownOpen && Items.Count > 0)
               SelectedItem = Items[0];

            IsDropDownOpen = false;
            e.Handled = true;
         }
         else if (e.Key == Key.Escape)
         {
            IsDropDownOpen = false;
            e.Handled = true;
         }
      }

      private void OnUnloaded(object sender, RoutedEventArgs e)
      {
         if (_editableTextBoxCache != null!)
            _editableTextBoxCache.KeyDown -= OnEditableTextBoxKeyDown; // Unhook keydown

         Unloaded -= OnUnloaded;
      }

      /// <summary>
      /// Gets text to match with the query from an item.
      /// Never null.
      /// </summary>
      /// <param name="item"/>
      private string TextFromItem(object item)
      {
         if (item == null!)
            return string.Empty;

         // Lazy initialization of the delegate
         if (_textFromItemDelegate == null ||
             (_itemType == null && _fullItemsSource != null && _fullItemsSource.Cast<object>().Any()))
            InitializeTextFromItemDelegate();

         return _textFromItemDelegate?.Invoke(item) ?? string.Empty;
      }

      private void InitializeTextFromItemDelegate()
      {
         _textFromItemDelegate = null;
         _itemType = null;

         var textPath = _cachedTextSearchTextPath;
         var memberPath = _cachedDisplayMemberPath ?? textPath;

         if (_fullItemsSource != null)
            _itemType = _fullItemsSource.Cast<object>().FirstOrDefault()?.GetType();

         if (_itemType == null || string.IsNullOrEmpty(memberPath))
         {
            _textFromItemDelegate = item => item.ToString() ?? string.Empty;
            return;
         }

         var parameter = Parameter(typeof(object), "item");
         var typedParameter = Convert(parameter, _itemType);
         var propertyAccess = PropertyOrField(typedParameter, memberPath);
         var nullCheck = Condition(Equal(propertyAccess, Constant(null, propertyAccess.Type)),
                                   Constant(string.Empty),
                                   Call(propertyAccess, typeof(object).GetMethod(nameof(object.ToString))!));
         var nullItemCheck = Condition(Equal(parameter, Constant(null)),
                                       Constant(string.Empty),
                                       nullCheck);

         var lambda = Lambda<Func<object, string>>(nullItemCheck, parameter);
         _textFromItemDelegate = lambda.Compile();
      }

      #region ItemsSource

      public static readonly DependencyProperty FullItemsSourceProperty =
         DependencyProperty.Register(nameof(FullItemsSource),
                                     typeof(IEnumerable),
                                     typeof(AutoCompleteComboBox),
                                     new(null, FullItemsSourcePropertyChanged));

      public IEnumerable FullItemsSource
      {
         get => (IEnumerable)GetValue(FullItemsSourceProperty);
         init => SetValue(FullItemsSourceProperty, value);
      }

      static AutoCompleteComboBox()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteComboBox),
                                                  new FrameworkPropertyMetadata(typeof(AutoCompleteComboBox)));
      }

      public AutoCompleteComboBox()
      {
         AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
         Unloaded += OnUnloaded;
      }

      private static void FullItemsSourcePropertyChanged(DependencyObject dependencyObject,
                                                         DependencyPropertyChangedEventArgs dpcea)
      {
         var comboBox = (AutoCompleteComboBox)dependencyObject;
         comboBox._fullItemsSource = dpcea.NewValue as IEnumerable;

         comboBox._cachedTextSearchTextPath = TextSearch.GetTextPath(comboBox);
         comboBox._cachedDisplayMemberPath = comboBox.DisplayMemberPath;
         comboBox._textFromItemDelegate = null;
         comboBox._itemType = null;
         comboBox.InitializeTextFromItemDelegate();

         if (comboBox.IsLoaded == false)
            comboBox.ItemsSource = comboBox._fullItemsSource;
      }

      #endregion ItemsSource

      #region Setting

      public static DependencyProperty SettingProperty { get; } = DependencyProperty.Register(nameof(Setting),
       typeof(AutoCompleteComboBoxSetting),
       typeof(AutoCompleteComboBox));

      public AutoCompleteComboBoxSetting? Setting
      {
         get => (AutoCompleteComboBoxSetting)GetValue(SettingProperty);
         set => SetValue(SettingProperty, value);
      }

      private AutoCompleteComboBoxSetting SettingOrDefault => Setting ?? AutoCompleteComboBoxSetting.Default;

      #endregion

      #region OnTextChanged and Filtering Logic (Modified)

      private long _revisionId;
      private string _previousText = null!;

      private struct TextBoxStatePreserver(TextBox textBox) : IDisposable
      {
         private readonly int _selectionStart = textBox.SelectionStart;
         private readonly int _selectionLength = textBox.SelectionLength;
         private readonly string _text = textBox.Text;

         public void Dispose()
         {
            textBox.Text = _text;
            textBox.Select(_selectionStart, _selectionLength);
         }
      }

      private async void ApplyFilter(string currentText)
      {
         if (IsDropdownOnly)
         {
            // Should never reach here but just in case
            ItemsSource = FullItemsSource;
            return;
         }

         var setting = SettingOrDefault;
         var maxSuggestionCount = setting.MaxSuggestionCount;
         var currentFilterDelegate = setting.GetFilter(currentText, TextFromItem);

         await _filterCancellationTokenSource.CancelAsync();
         _filterCancellationTokenSource = new();
         var token = _filterCancellationTokenSource.Token;

         IEnumerable<object>? filteredItems;

         if (string.IsNullOrEmpty(currentText))
            filteredItems = _fullItemsSource?.Cast<object>() ?? [];
         else
            filteredItems = await Task.Run(GetFilteredItems, token);

         if (token.IsCancellationRequested)
            return;

         Dispatcher.Invoke(() =>
         {
            var itemsSource = filteredItems.ToList();
            using (new TextBoxStatePreserver(EditableTextBox))
               ItemsSource = itemsSource;

            if (filteredItems != null && itemsSource.Count > 0 && EditableTextBox.IsFocused)
               IsDropDownOpen = true;
            else
               IsDropDownOpen = false;

            Unselect();
         });
         return;

         IEnumerable<object> GetFilteredItems()
         {
            if (_fullItemsSource == null)
               return [];

            var fullItems = _fullItemsSource.Cast<object>();

            var results = new List<object>();
            foreach (var item in fullItems)
            {
               if (token.IsCancellationRequested)
                  return [];

               if (currentFilterDelegate(item))
               {
                  results.Add(item);
                  if (results.Count >= maxSuggestionCount)
                     break;
               }
            }

            return results;
         }
      }

      private void Unselect()
      {
         if (!EditableTextBox.IsKeyboardFocusWithin)
            return;

         var textBox = EditableTextBox;
         if (textBox != null!)
            textBox.Select(textBox.SelectionStart + textBox.SelectionLength, 0);
      }

      protected override void OnSelectionChanged(SelectionChangedEventArgs e)
      {
         // When our flag is set, it means we are in the middle of typing.
         // We should NOT let the base ComboBox logic run, as it will
         // overwrite the user's text with an empty string (from the null SelectedItem).
         if (_isUpdatingText)
            return;

         // Use a flag to signal that the subsequent TextChanged event is from a selection, not typing.
         _isUpdatingText = true;
         base.OnSelectionChanged(e); // This will update the Text property
         _isUpdatingText = false;
      }

      private void OnTextChanged(object sender, TextChangedEventArgs e)
      {
         if (!_isInitialized)
            return;
         
         if (IsDropdownOnly)
         {
            Unselect();
            return;
         }

         if (_isUpdatingText)
            return;

         // Set SelectedItem to null to indicate that the text no longer matches a selected item.
         // Use the flag to prevent the OnSelectionChanged handler from clearing the text.
         _isUpdatingText = true;
         SelectedItem = null;
         _isUpdatingText = false;

         var id = unchecked(++_revisionId);
         var setting = SettingOrDefault;
         var currentText = Text.Trim();

         if (string.Equals(currentText, _previousText, StringComparison.Ordinal) &&
             IsDropDownOpen)
            return;

         _previousText = currentText;

         if (setting.Delay <= TimeSpan.Zero)
         {
            ApplyFilter(currentText);
            return;
         }

         _disposable.Content =
            new Timer(_ =>
                      {
                         Dispatcher.InvokeAsync(() =>
                         {
                            if (_revisionId != id)
                               return;

                            ApplyFilter(currentText);
                         });
                      },
                      null,
                      setting.Delay,
                      Timeout.InfiniteTimeSpan);
      }

      #endregion

      #region Focus Handling

      protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
      {
         base.OnGotKeyboardFocus(e);
         if (IsDropdownOnly && Equals(e.OriginalSource, this))
         {
            IsDropDownOpen = true;
            Unselect();
         }
      }

      protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
      {
         base.OnLostKeyboardFocus(e);
         if (IsDropdownOnly)
         {
            IsDropDownOpen = false;
            Unselect();
         }
      }

      #endregion
   }
}