using System.Net.Http;
using Octokit;

namespace Arcanum.Core.Utils.Git;

public static class GitDataService
{
   public const string MOD_FORGE_LATEST_VERSION_KEY = "ModForgeLatestVersion";
   public const string GIT_OWNER = "Minnator";
   public const string MOD_FORGE_GIT_REPOSITORY = "Minnators-Modforge";

   public const string ARCANUM_GIT_REPOSITORY = "Arcanum";
   public const string ARCANUM_GIT_OWNER = "The-Arcanum-Project";
   public const string ARCANUM_LATEST_VERSION_KEY = "ArcanumLatestVersion";

   public const string ARCANUM_REPOSITORY_URL = "https://github.com/The-Arcanum-Project/Arcanum";

   public const string ARCANUM_RELEASES_URL = ARCANUM_REPOSITORY_URL + "/releases";

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

   public static GitReleaseObject GetLatestVersion(string repoName, string repoOwner, string dataKey, GitReleaseObject? latestVersion)
   {
      if (latestVersion == null || latestVersion.DataKey != dataKey || latestVersion.RepositoryOwner != repoOwner || latestVersion.RepositoryName != repoName)
      {
         latestVersion = new()
         {
            RepositoryName = repoName,
            RepositoryOwner = repoOwner,
            DataKey = dataKey,
         };
      }

      // We still have data and it is not outdated, return it
      if (latestVersion.IsDataAvailable() && !latestVersion.IsDataOutdated)
         return latestVersion;
      
      latestVersion.LastFetch = DateTime.Now;
      
      var client = CreateClient(repoOwner);

      try
      {
         var latestRelease = client.Repository.Release.GetLatest(repoOwner, repoName).Result;

         ArcLog.WriteLine("GDS", LogLevel.INF, "GitDataService: Fetched latest release from GitHub");
         
         latestVersion.Data = new()
         {
            Name = latestRelease.Name,
            TagName = latestRelease.TagName,
            Body = latestRelease.Body,
         };
      }
      catch (NotFoundException e)
      {
         ArcLog.WriteLine("GDS", LogLevel.ERR, "GitDataService: Unable to find the specified repository or releases.");
         ArcLog.WriteLine("GDS", LogLevel.ERR, e.ToString());
      }
      catch (AggregateException e)
      {
         ArcLog.WriteLine("GDS", LogLevel.ERR, "GitDataService: Error fetching latest release from GitHub.");
         ArcLog.WriteLine("GDS", LogLevel.ERR, e.ToString());
      }

      return latestVersion;
   }

   private static GitHubClient CreateClient(string gitOwner) => new(new ProductHeaderValue(gitOwner));
}