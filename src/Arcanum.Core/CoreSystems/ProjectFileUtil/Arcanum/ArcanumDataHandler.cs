using System.IO;
using System.Text;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.Utils.Git;

namespace Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;

public static class ArcanumDataHandler
{
   public static void LoadDefaultDescriptor(ArcanumDataDescriptor descriptor)
   {
      LoadGitData();
   }

   private static void GetFromAppData(string internalPath, out string? data)
   {
      var arcanumDataPath = IO.IO.GetArcanumDataPath;
      var filePath = Path.Combine(arcanumDataPath, internalPath);

      data = IO.IO.ReadAllText(filePath, Encoding.UTF8);
   }

   private static void LoadGitData()
   {
      GetFromAppData(AppData.ModforgeDataDescriptor.InternalPathLatestRelease, out var latestModForgeRelease);

      if (latestModForgeRelease is null)
         AppData.ModforgeDataDescriptor.LatestVersion =
            GitDataService.GetLatestVersion(GitDataService.MOD_FORGE_GIT_REPOSITORY,
                                            GitDataService.GIT_OWNER,
                                            GitDataService.MOD_FORGE_LATEST_VERSION_KEY);
      else
      {
         var gitDataObject = JsonProcessor.Deserialize<GitReleaseObject>(latestModForgeRelease) ??
                             GitDataService.GetLatestVersion(GitDataService.MOD_FORGE_GIT_REPOSITORY,
                                                             GitDataService.GIT_OWNER,
                                                             GitDataService.MOD_FORGE_LATEST_VERSION_KEY);
         AppData.ModforgeDataDescriptor.LatestVersion = gitDataObject;
      }

      GetFromAppData(AppData.ArcanumDataDescriptor.InternalPathLatestRelease, out var latestArcanumRelease);

      if (latestArcanumRelease is null)
         AppData.ArcanumDataDescriptor.LatestVersion =
            GitDataService.GetLatestVersion(GitDataService.ARCANUM_GIT_REPOSITORY,
                                            GitDataService.ARCANUM_GIT_OWNER,
                                            GitDataService.ARCANUM_LATEST_VERSION_KEY);
      else
      {
         var gdo = JsonProcessor.Deserialize<GitReleaseObject>(latestArcanumRelease) ??
                   GitDataService.GetLatestVersion(GitDataService.ARCANUM_GIT_REPOSITORY,
                                                   GitDataService.ARCANUM_GIT_OWNER,
                                                   GitDataService.ARCANUM_LATEST_VERSION_KEY);
         AppData.ArcanumDataDescriptor.LatestVersion = gdo;
      }
   }

   #region Saving

   public static void SaveAllGitData()
   {
      var gitDescriptor = AppData.ModforgeDataDescriptor;
      if (gitDescriptor.LatestVersion is not null)
      {
         var latestReleaseJson = JsonProcessor.Serialize(gitDescriptor.LatestVersion);
         var filePath = Path.Combine(IO.IO.GetArcanumDataPath, gitDescriptor.InternalPathLatestRelease);
         IO.IO.WriteAllText(filePath, latestReleaseJson, Encoding.UTF8);
      }

      var arcanumGitDescriptor = AppData.ArcanumDataDescriptor;
      if (arcanumGitDescriptor.LatestVersion is not null)
      {
         var latestReleaseJson = JsonProcessor.Serialize(arcanumGitDescriptor.LatestVersion);
         var filePath = Path.Combine(IO.IO.GetArcanumDataPath, arcanumGitDescriptor.InternalPathLatestRelease);
         IO.IO.WriteAllText(filePath, latestReleaseJson, Encoding.UTF8);
      }
   }

   #endregion
}