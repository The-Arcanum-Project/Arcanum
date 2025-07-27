using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.Settings;

namespace Arcanum.Core.Globals;

public static class Globals
{
   public static MainSettingsObj Settings { get; set; } = new ();
   public static TreeHistoryManager HistoryManager { get; set; } = new TreeHistoryManager(new ());
}