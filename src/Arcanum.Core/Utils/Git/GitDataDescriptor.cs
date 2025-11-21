namespace Arcanum.Core.Utils.Git;

public class GitDataDescriptor(string repoName)
{
   // Cached data from the git repository:
   // - Latest release
   // - Featured feature 
   // - Wiki

   public string GitRepoName { get; set; } = repoName;
   public string InternalPathLatestRelease => $"Git/{GitRepoName}latest_release.json";
   public GitReleaseObject? LatestVersion { get; set; }
}