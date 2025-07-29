using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.Settings;

namespace Arcanum.Core.GlobalStates;

public enum AppState
{
   Error,
   Loading,
   EditingAllowed,
   EditingDisabled,
   Saving,
}

public static class Globals
{
   #region State of the application

   public static AppState AppState = AppState.Loading;

   #endregion
   
   public static MainSettingsObj Settings { get; set; } = new ();
   public static TreeHistoryManager HistoryManager { get; set; } = new (new ());
}