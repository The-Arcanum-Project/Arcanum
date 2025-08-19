using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.Utils;
using Arcanum.Core.Utils.Git;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class HomeViewModel : UserControl
{
   public HomeViewModel()
   {
      InitializeComponent();

      var latestNews = GitDataService.GetReleaseNotesForVersion("v1.0.0-beta", "Minnator", "Arcanum", "main");
      LatestNewsText.Text = latestNews;
   }
   
   
   private void DiscordTextBlock_PreviewMouseUp(object sender, MouseButtonEventArgs e)
   {
      if (e.ChangedButton == MouseButton.Left)
      {
         e.Handled = true;
         ProcessHelper.OpenDiscordLinkIfDiscordRunning(GitDataService.MODFORGE_DISCORD_URL);
      }
   }

   private void GitHubTextBlock_PreviewMouseUp(object sender, MouseButtonEventArgs e)
   {
      if (e.ChangedButton == MouseButton.Left)
      {
         e.Handled = true;
         ProcessHelper.OpenLink(GitDataService.ARCANUM_REPOSITORY_URL);
      }
   }
}