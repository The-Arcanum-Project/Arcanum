using System.Windows.Input;
using System.Windows.Navigation;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class AttributionsViewModel
{
   public AttributionsViewModel()
   {
      InitializeComponent();
      
      Bby40LicenseLink.RequestNavigate += Hyperlink_OnRequestNavigate;
   }

   private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
   {
      ProcessHelper.OpenLink(e.Uri.ToString());
   }

   private void BBY40LicenseLink_OnMouseUp(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(Bby40LicenseLink.NavigateUri.AbsolutePath);
   }
}