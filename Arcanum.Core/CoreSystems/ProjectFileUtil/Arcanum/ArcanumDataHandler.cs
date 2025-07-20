using System.IO;
using System.Text;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.Globals;
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
      GetFromAppData(AppData.GitDataDescriptor.InternalPathLatestRelease, out var latestReleaseJson);

      if (latestReleaseJson is null)
         AppData.GitDataDescriptor.LatestVersion = GitDataService.GetLatestVersion();
      else
      {
         var gitDataObject = JsonProcessor.Deserialize<GitReleaseObject>(latestReleaseJson, null) ??
                             GitDataService.GetLatestVersion();
         AppData.GitDataDescriptor.LatestVersion = gitDataObject;
      }
   }

   #region Saving

   public static void SaveAllGitData(ArcanumDataDescriptor descriptor)
   {
      var gitDescriptor = AppData.GitDataDescriptor;
      if (gitDescriptor.LatestVersion is not null)
      {
         var latestReleaseJson = JsonProcessor.Serialize(gitDescriptor.LatestVersion);
         var filePath = Path.Combine(IO.IO.GetArcanumDataPath, gitDescriptor.InternalPathLatestRelease);
         IO.IO.WriteAllText(filePath, latestReleaseJson, Encoding.UTF8);
      }
   }


   #endregion
}