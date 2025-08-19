using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class AttributionsViewModel : UserControl
{
   public AttributionsViewModel()
   {
      InitializeComponent();
      
      BBY40LicenseLink.RequestNavigate += Hyperlink_OnRequestNavigate;
   }

   private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
   {
      ProcessHelper.OpenLink(e.Uri.ToString());
   }

   private void BBY40LicenseLink_OnMouseUp(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(BBY40LicenseLink.NavigateUri.AbsolutePath);
   }
}