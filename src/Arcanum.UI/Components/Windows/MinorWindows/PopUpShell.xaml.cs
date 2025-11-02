using System.Windows;
using System.Windows.Controls;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class PopUpShell
{
   public PopUpShell()
   {
      InitializeComponent();
   }

   public UserControl HostedContent
   {
      get => (ContentHost.Content as UserControl)!;
      set => ContentHost.Content = value;
   }

   public string DialogTitle
   {
      get => TitleTextBlock.Text;
      set => TitleTextBlock.Text = value;
   }

   private void Confirm_Click(object sender, RoutedEventArgs e)
   {
      DialogResult = true;
      Close();
   }

   private void Cancel_Click(object sender, RoutedEventArgs e)
   {
      DialogResult = false;
      Close();
   }
}