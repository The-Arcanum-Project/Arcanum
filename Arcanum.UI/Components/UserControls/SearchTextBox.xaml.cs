using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace Arcanum.UI.Components.UserControls;

public partial class SearchTextBox
{
   public Action<string> RequestSearch { get; set; } = null!;
   public Action SettingsOpened { get; set; } = null!;
   private readonly Timer _searchTimer;

   public SearchTextBox()
   {
      _searchTimer = new(250);
      _searchTimer.Elapsed += (_, _) =>
      {
         _searchTimer.Stop();
         if (RequestSearch == null!)
            throw new InvalidOperationException("RequestSearch action is not set.");

         Dispatcher.Invoke(() => { RequestSearch.Invoke(SearchInputTextBox.Text); });
      };
      InitializeComponent();
   }

   private void SearchInputTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
   {
      if (_searchTimer.Enabled)
         _searchTimer.Stop();
      _searchTimer.Start();
   }

   private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
   {
      SettingsOpened.Invoke();
      RequestSearch.Invoke(SearchInputTextBox.Text);
   }
}