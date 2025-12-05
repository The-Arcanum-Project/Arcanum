using System.Reflection;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.Utils.Git;

namespace Arcanum.Core.GlobalStates;

public static class AppData
{
   public static bool IsHeadless { get; set; } = false;
   private static readonly Assembly Assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

   public static string AppVersion { get; } = Assembly.GetName().Version?.ToString()[..^2] ?? "0.0.0";
   public static string ProductName { get; } =
      Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Arcanum_INVALID_PRODUCT";

   public static string FullTitle { get; } = $"{ProductName} v{AppVersion}";

   public static GitDataDescriptor ModforgeDataDescriptor { get; } = new(GitDataService.MOD_FORGE_GIT_REPOSITORY);
   public static GitDataDescriptor ArcanumDataDescriptor { get; } = new(GitDataService.ARCANUM_GIT_REPOSITORY);
   public static MainMenuScreenDescriptor MainMenuScreenDescriptor { get; set; } = null!;

   public static QueastorSearchSettings QueastorSearchSettings { get; set; } = new();

   #region State of the application

   public static AppState AppState = AppState.Loading;

   #endregion

   public static TreeHistoryManager HistoryManager { get; set; } = new(new());
}