using Arcanum.API.UtilServices;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.Utils.Git;

namespace Arcanum.Core.GlobalStates;

public static class AppData
{
   internal const string APP_NAME = "Arcanum";
   internal const string APP_VERSION = "1.0.0-beta";

   public static GitDataDescriptor GitDataDescriptor { get; } = new();
   public static ArcanumDataDescriptor DataDescriptor { get; set; } = null!;
   public static MainMenuScreenDescriptor MainMenuScreenDescriptor { get; set; } = null!;
   
   public static ProjectFileDescriptor CurrentProjectFile { get; set; } = null!;
   
   public static SearchSettings SearchSettings { get; set; } = new();
   
   public static IWindowLinker WindowLinker { get; set; } = null!;
}
