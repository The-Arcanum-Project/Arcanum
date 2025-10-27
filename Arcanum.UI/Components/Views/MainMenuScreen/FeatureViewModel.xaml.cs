using System.Windows.Input;
using Arcanum.Core.Utils.Git;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class FeatureViewModel
{
   public FeatureViewModel()
   {
      InitializeComponent();
   }

   private void OpenUserGuide(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(GitDataService.ARCANUM_USER_GUIDE_URL);
   }

   private void OpenDevGuide(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(GitDataService.ARCANUM_DEV_DOCUMENTATION_URL);
   }
}