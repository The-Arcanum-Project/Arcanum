using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.ProjectFileUtil.Arcanum;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.Utils.Git;

namespace Arcanum.Core.GlobalStates;

public static class AppData
{
   internal const string APP_NAME = "Arcanum";
   internal const string APP_VERSION = "1.0.0-beta";

   public static GitDataDescriptor GitDataDescriptor { get; } = new();
   public static MainMenuScreenDescriptor MainMenuScreenDescriptor { get; set; } = null!;

   public static QueastorSearchSettings QueastorSearchSettings { get; set; } = new();

   #region State of the application

   public static AppState AppState = AppState.Loading;

   #endregion

   public static TreeHistoryManager HistoryManager { get; set; } = new(new());
}