using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.Utils;
using Arcanum.Core.Utils.Git;
using Common;

namespace Arcanum.UI.Components.Views.MainMenuScreen;

public partial class AboutUsViewModel : UserControl
{
   public AboutUsViewModel()
   {
      InitializeComponent();
   }

   private void OpenPatreonButton_Click(object sender, RoutedEventArgs e)
   {
      ProcessHelper.OpenLink("https://www.patreon.com/c/Minnator");
   }

   private void OpenGithubButton_OnClick(object sender, RoutedEventArgs e)
   {
      ProcessHelper.OpenLink(GitDataService.ARCANUM_REPOSITORY_URL);
   }

   private void OpenDiscordButton_OnClick(object sender, RoutedEventArgs e)
   {
      ProcessHelper.OpenDiscordLinkIfDiscordRunning(GitDataService.MODFORGE_DISCORD_URL);
   }
}