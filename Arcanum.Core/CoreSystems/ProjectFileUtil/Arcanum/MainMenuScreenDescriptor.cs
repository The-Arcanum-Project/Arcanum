using System.IO;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;

public class MainMenuScreenDescriptor
{
   public const string INTERNAL_PATH_MAIN_MENU_SCREEN = "MM_Screen/main_menu_screen.json";

   public List<ProjectFileDescriptor> ProjectFiles { get; set; } = [];
   public string? LastProjectFile { get; set; } = null;
   public DataSpace? LastVanillaPath { get; set; } = null;

   /// <summary>
   /// Only meant for serialization purposes.
   /// </summary>
   public MainMenuScreenDescriptor()
   {
   }

   // We do not need an instance here as we can use the instance of th AppData class
   public static void SaveData()
   {
      var path = Path.Combine(IO.IO.GetArcanumDataPath, INTERNAL_PATH_MAIN_MENU_SCREEN);
      var json = JsonProcessor.Serialize(AppData.MainMenuScreenDescriptor);
      IO.IO.WriteAllTextAnsi(path, json);
   }

   public static void LoadData()
   {
      var path = Path.Combine(IO.IO.GetArcanumDataPath, INTERNAL_PATH_MAIN_MENU_SCREEN);
      if (File.Exists(path))
      {
         var json = IO.IO.ReadAllTextAnsi(path);
         AppData.MainMenuScreenDescriptor = JsonProcessor.Deserialize<MainMenuScreenDescriptor>(json ?? string.Empty) ??
                                            new MainMenuScreenDescriptor();
      }
      else
      {
         AppData.MainMenuScreenDescriptor = new();
      }
   }

   public ProjectFileDescriptor? GetLastDescriptor()
   {
      return ProjectFiles.FirstOrDefault(x => x?.ModName.Equals(LastProjectFile) ?? false, null);
   }
}