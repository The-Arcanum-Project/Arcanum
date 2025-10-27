using System.Windows.Input;
using System.Windows.Navigation;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class AttributionsViewModel
{
   public AttributionsViewModel()
   {
      InitializeComponent();
      
   }

   private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
   {
      ProcessHelper.OpenLink(e.Uri.ToString());
   }
}