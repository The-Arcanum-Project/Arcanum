using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox
{
   /// <summary>
   /// AutoCompleteComboBox.xaml
   /// </summary>
   public class AutoCompleteComboBox : ComboBox
   {
      private readonly SerialDisposable _disposable = new();

      private TextBox _editableTextBoxCache = null!;

      private Predicate<object> _defaultItemsFilter = null!;

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
      

      /// <summary>
      /// Gets text to match with the query from an item.
      /// Never null.
      /// </summary>
      /// <param name="item"/>
      private string TextFromItem(object item)
      {
         if (item == null!)
            return string.Empty;

         var d = new DependencyVariable<string>();
         d.SetBinding(item, TextSearch.GetTextPath(this));
         return d.Value;
      }

      #region ItemsSource

      public new static readonly DependencyProperty ItemsSourceProperty =
         DependencyProperty.Register(nameof(ItemsSource),
                                     typeof(IEnumerable),
                                     typeof(AutoCompleteComboBox),
                                     new(null, ItemsSourcePropertyChanged));

      public new IEnumerable ItemsSource
      {
         get => (IEnumerable)GetValue(ItemsSourceProperty);
         init => SetValue(ItemsSourceProperty, value);
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

      private static void ItemsSourcePropertyChanged(DependencyObject dependencyObject,
                                                     DependencyPropertyChangedEventArgs dpcea)
      {
         var comboBox = (ComboBox)dependencyObject;
         var previousSelectedItem = comboBox.SelectedItem;

         if (dpcea.NewValue is ICollectionView cv)
         {
            ((AutoCompleteComboBox)dependencyObject)._defaultItemsFilter = cv.Filter;
            comboBox.ItemsSource = cv;
         }
         else
         {
            ((AutoCompleteComboBox)dependencyObject)._defaultItemsFilter = null!;
            var newValue = dpcea.NewValue as IEnumerable;
            var newCollectionViewSource = new CollectionViewSource { Source = newValue };
            comboBox.ItemsSource = newCollectionViewSource.View;
         }

         comboBox.SelectedItem = previousSelectedItem;

         // if ItemsSource doesn't contain previousSelectedItem
         if (comboBox.SelectedItem != previousSelectedItem)
            comboBox.SelectedItem = null;
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

      #region OnTextChanged

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

      private static int CountWithMax<T>(IEnumerable<T> xs, Predicate<T> predicate, int maxCount)
      {
         var count = 0;
         foreach (var x in xs)
            if (predicate(x))
            {
               count++;
               if (count > maxCount)
                  return count;
            }

         return count;
      }

      private void Unselect()
      {
         var textBox = EditableTextBox;
         textBox.Select(textBox.SelectionStart + textBox.SelectionLength, 0);
      }

      private void UpdateFilter(Predicate<object> filter)
      {
         using (new TextBoxStatePreserver(EditableTextBox))
            using (Items.DeferRefresh())
               // Can empty the text box. I don't why.
               Items.Filter = filter;
      }

      private void OpenDropDown(Predicate<object> filter)
      {
         UpdateFilter(filter);
         IsDropDownOpen = true;
         Unselect();
      }

      private void UpdateSuggestionList()
      {
         var text = Text;

         if (text == _previousText)
            return;

         _previousText = text;

         if (string.IsNullOrEmpty(text))
         {
            IsDropDownOpen = false;
            SelectedItem = null;

            using (Items.DeferRefresh())
               Items.Filter = _defaultItemsFilter;
         }
         else if (SelectedItem != null && TextFromItem(SelectedItem) == text)
         {
            // It seems the user selected an item.
            // Do nothing.
         }
         else
         {
            using (new TextBoxStatePreserver(EditableTextBox))
               SelectedItem = null;

            var filter = GetFilter();
            var maxCount = SettingOrDefault.MaxSuggestionCount;
            var count = CountWithMax(ItemsSource.Cast<object>(), filter, maxCount);

            if (0 < count && count <= maxCount)
               OpenDropDown(filter);
         }
      }

      private void OnTextChanged(object sender, TextChangedEventArgs e)
      {
         var id = unchecked(++_revisionId);
         var setting = SettingOrDefault;

         if (setting.Delay <= TimeSpan.Zero)
         {
            UpdateSuggestionList();
            return;
         }

         _disposable.Content =
            new Timer(_ =>
                      {
                         Dispatcher.InvokeAsync(() =>
                         {
                            if (_revisionId != id)
                               return;

                            UpdateSuggestionList();
                         });
                      },
                      null,
                      setting.Delay,
                      Timeout.InfiniteTimeSpan);
      }

      #endregion

      private Predicate<object> GetFilter()
      {
         var filter = SettingOrDefault.GetFilter(Text, TextFromItem);

         return _defaultItemsFilter != null!
                   ? i => _defaultItemsFilter(i) && filter(i)
                   : filter;
      }
   }
}