using System.Windows.Input;
using Arcanum.Core.Utils.Git;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class HomeViewModel
{
   public HomeViewModel()
   {
      InitializeComponent();

      //var latestNews = GitDataService.GetReleaseNotesForVersion("v1.0.0-beta", "Minnator", "Arcanum", "main");
      //LatestNewsText.Text = latestNews;
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

   private void OpenGithub(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(GitDataService.ARCANUM_REPOSITORY_URL);
   }

   private void OpenDiscord(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenDiscordLinkIfDiscordRunning(GitDataService.MODFORGE_DISCORD_URL);
   }

   private void OpenDocumentation(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(GitDataService.ARCANUM_USER_GUIDE_URL);
   }

   private void OpenRelease(object sender, MouseButtonEventArgs e)
   {
      ProcessHelper.OpenLink(GitDataService.ARCANUM_RELEASES_URL);
   }
}