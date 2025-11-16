using System.Net.Http;
using Octokit;

namespace Arcanum.Core.Utils.Git;

public static class GitDataService
{
   public const string MOD_FORGE_LATEST_VERSION_KEY = "ModForgeLatestVersion";
   private const string GIT_OWNER = "Minnator";
   private const string MOD_FORGE_GIT_REPOSITORY = "Minnators-Modforge";
   public const string ARCANUM_REPOSITORY_URL = "https://github.com/The-Arcanum-Project/Arcanum";

   public const string ARCANUM_RELEASES_URL = ARCANUM_REPOSITORY_URL + "/releases";

   //TODO: @MelCo Update documentation URL when docs are available
   public const string ARCANUM_DOCUMENTATION_URL = "https://the-arcanum-project.github.io/Arcanum";
   public const string ARCANUM_USER_GUIDE_URL = ARCANUM_DOCUMENTATION_URL + "/user/about-arcanum.html";
   public const string ARCANUM_DEV_DOCUMENTATION_URL = ARCANUM_DOCUMENTATION_URL + "/dev/about-arcanum.html";
   public const string MODFORGE_REPOSITORY_URL = "https://github.com/Minnator/Minnators-Modforge";

   public const string MODFORGE_DISCORD_URL = "https://discord.gg/22AhD5qkme";

   private const string RELEASE_NOTES_FILE_PATH = "Arcanum.Nexus.Core/ReleaseNotes";

   public static string GetFileFromRepositoryUrl(string owner, string repository, string branch, string filePath)
   {
      var client = new HttpClient();
      var url = $"https://raw.githubusercontent.com/{owner}/{repository}/{branch}/{filePath}";

      var result = client.GetAsync(url).Result;
      if (result.IsSuccessStatusCode)
         return result.Content.ReadAsStringAsync().Result;

      return "Unable to fetch file";
   }

   public static string GetReleaseNotesForVersion(string version, string owner, string repository, string branch)
   {
      var filePath = $"{RELEASE_NOTES_FILE_PATH}/{version}.md";
      return GetFileFromRepositoryUrl(owner, repository, branch, filePath);
   }

   public static GitReleaseObject GetLatestVersion()
   {
      var gdo = AppData.GitDataDescriptor.LatestVersion ??
                new()
                {
                   RepositoryName = MOD_FORGE_GIT_REPOSITORY,
                   RepositoryOwner = GIT_OWNER,
                   DataKey = MOD_FORGE_LATEST_VERSION_KEY,
                };

      // We still have data and it is not outdated, return it
      if (gdo.IsDataAvailable() && !gdo.IsDataOutdated)
         return gdo;

      var client = CreateClient();
      var latestRelease = client.Repository.Release.GetLatest(GIT_OWNER, MOD_FORGE_GIT_REPOSITORY).Result;

      Console.WriteLine("GitDataService: Fetched latest release from GitHub");

      gdo.Data = new()
      {
         Name = latestRelease.Name,
         TagName = latestRelease.TagName,
         Body = latestRelease.Body,
      };

      return gdo;
   }

   private static GitHubClient CreateClient() => new(new ProductHeaderValue(GIT_OWNER));
}