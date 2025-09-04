using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

      public override void OnApplyTemplate()
      {
         base.OnApplyTemplate();
         _editableTextBoxCache =
            GetTemplateChild("PART_EditableTextBox") as TextBox ?? throw new InvalidOperationException();
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
            _textFromItemDelegate = item => item?.ToString() ?? string.Empty;
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
      }

      private static void FullItemsSourcePropertyChanged(DependencyObject dependencyObject,
                                                         DependencyPropertyChangedEventArgs dpcea)
      {
         var comboBox = (AutoCompleteComboBox)dependencyObject;
         comboBox._fullItemsSource = dpcea.NewValue as IEnumerable;
         comboBox._cachedTextSearchTextPath = TextSearch.GetTextPath(comboBox);
         comboBox._cachedDisplayMemberPath = comboBox.DisplayMemberPath; // Call on UI thread
         comboBox._textFromItemDelegate = null;
         comboBox._itemType = null;
         comboBox.InitializeTextFromItemDelegate();
         comboBox.ApplyFilter(comboBox.Text);
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
            filteredItems = await Task.Run(() =>
                                           {
                                              if (_fullItemsSource == null)
                                                 return [];

                                              var fullItems = _fullItemsSource.Cast<object>();

                                              var results = new List<object>();
                                              foreach (var item in fullItems)
                                              {
                                                 if (token.IsCancellationRequested) 
                                                    return Enumerable.Empty<object>(); 

                                                 if (currentFilterDelegate(item))
                                                 {
                                                    results.Add(item);
                                                    if (results.Count >= maxSuggestionCount)
                                                       break; 
                                                 }
                                              }

                                              return results;
                                           },
                                           token); 

         if (token.IsCancellationRequested)
            return;

         Dispatcher.Invoke(() =>
         {
            var itemsSource = filteredItems.ToList();
            ItemsSource = itemsSource; 

            if (string.IsNullOrEmpty(currentText))
            {
               IsDropDownOpen = true;
               SelectedItem = null;
            }
            else
            {
               if (filteredItems != null && itemsSource.Count != 0)
               {
                  IsDropDownOpen = true;
                  if (SelectedItem != null &&
                      !TextFromItem(SelectedItem).Equals(currentText, StringComparison.OrdinalIgnoreCase))
                     using (new TextBoxStatePreserver(EditableTextBox))
                        SelectedItem = null;
               }
               else
               {
                  IsDropDownOpen = false;
                  SelectedItem = null;
               }
            }

            Unselect(); 
         });
      }

      private void Unselect()
      {
         var textBox = EditableTextBox;
         if (textBox != null!) 
            textBox.Select(textBox.SelectionStart + textBox.SelectionLength, 0);
      }

      private void OnTextChanged(object sender, TextChangedEventArgs e)
      {
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
   }
}