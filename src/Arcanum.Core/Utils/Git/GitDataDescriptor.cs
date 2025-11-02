namespace Arcanum.Core.Utils.Git;

public class GitDataDescriptor
{
   // Cached data from the git repository:
   // - Latest release
   // - Featured feature 
   // - Wiki

   public string InternalPathLatestRelease => "Git/latest_release.json";
   public GitReleaseObject? LatestVersion { get; set; }
}