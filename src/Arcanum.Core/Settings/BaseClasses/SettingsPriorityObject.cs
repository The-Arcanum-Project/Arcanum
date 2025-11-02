namespace Arcanum.Core.Settings.BaseClasses;

public class SettingsPriorityObject(int priority, SettingsEventManager.SettingsEventHandler handler)
{
   /// <summary>
   /// The priority of this object in settings lists. Higher priority objects override lower priority ones.
   /// </summary>
   public int Priority { get; set; } = priority;

   /// <summary>
   /// The handler to be called when settings are applied.
   /// </summary>
   public SettingsEventManager.SettingsEventHandler Handler { get; set; } = handler;
}