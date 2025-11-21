using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.Utils.Git;

namespace Arcanum.Core.GlobalStates;

public static class AppData
{
   public const string APP_NAME = "Arcanum";
   public const string APP_VERSION = "0.9.2 Alpha";

   public static GitDataDescriptor ModforgeDataDescriptor { get; } = new(GitDataService.MOD_FORGE_GIT_REPOSITORY);
   public static GitDataDescriptor ArcanumDataDescriptor { get; } = new(GitDataService.ARCANUM_GIT_REPOSITORY);
   public static MainMenuScreenDescriptor MainMenuScreenDescriptor { get; set; } = null!;

   public static QueastorSearchSettings QueastorSearchSettings { get; set; } = new();

   #region State of the application

   public static AppState AppState = AppState.Loading;

   #endregion

   public static TreeHistoryManager HistoryManager { get; set; } = new(new());
}