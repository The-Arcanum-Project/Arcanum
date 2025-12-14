using System.Windows.Input;
using System.Windows.Media;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.Git;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class HomeViewModel
{
   public HomeViewModel()
   {
      InitializeComponent();
      SetReleaseText();
      //var latestNews = GitDataService.GetReleaseNotesForVersion("v1.0.0-beta", "Minnator", "Arcanum", "main");
      //LatestNewsText.Text = latestNews;
   }

   private void SetReleaseText()
   {
      var latestRelease = AppData.ArcanumDataDescriptor.LatestVersion;
      if (latestRelease is { Data: not null })
      {
         var releaseName = latestRelease.Data.Name.Split(' ');
         LatestReleaseName.Text = releaseName.Length > 2 ? $"{releaseName[0]} {releaseName[1]}" : latestRelease.Data.Name;


         LatestReleaseVersion.Text = latestRelease.Data.TagName;
         
         LatestReleaseName.ToolTip = $"Release notes:\n{latestRelease.Data.Body}";
         
         var releaseVersion = latestRelease.Data.TagName.StartsWith('v')
            ? latestRelease.Data.TagName[1..]
            : latestRelease.Data.TagName;

         var split = releaseVersion.Split("-");
         
         if (split.Length > 1)
            releaseVersion = split[0];
         
         var isCurrent = releaseVersion.Equals(AppData.AppVersion);
         
         LatestReleaseStatus.Text = isCurrent ? "Up to date" : "Update now!";
         if (!isCurrent)
         {
            LatestReleaseStatus.Foreground = Brushes.White;
            ColorA.Color = Colors.MediumOrchid;
            ColorB.Color = Colors.OrangeRed;
         }
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