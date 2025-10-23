namespace Arcanum.Core.Settings.BaseClasses;

public static class SettingsEventManager
{
   public static event SettingsEventHandler? OnSettingsChanged;
   public delegate void SettingsEventHandler(object sender, SettingsEventArgs e);

   private static void OnOnSettingsChanged(object sender, SettingsEventArgs eArgs)
   {
      OnSettingsChanged?.Invoke(sender, eArgs);
   }

   private static readonly Dictionary<string, SortedList<int, SettingsPriorityObject>> SettingsPriorityObjects = new();

   public static void RegisterSettingsHandler(string settingKey, SettingsEventHandler handler, int priority = 0)
   {
      if (!SettingsPriorityObjects.ContainsKey(settingKey))
         SettingsPriorityObjects[settingKey] = new();

      SettingsPriorityObjects[settingKey].Add(priority, new(priority, handler));
   }

   public static void UnregisterSettingsPriorityObject(string settingKey, SettingsEventHandler handler)
   {
      if (!SettingsPriorityObjects.TryGetValue(settingKey, out var list))
         return;

      var itemToRemove = list.Values.FirstOrDefault(item => item.Handler == handler);
      if (itemToRemove != null)
         list.Remove(itemToRemove.Priority);
   }

   internal static void TriggerSettingsChanged(object sender, SettingsEventArgs eArgs)
   {
      OnOnSettingsChanged(sender, eArgs);

      if (!SettingsPriorityObjects.TryGetValue(eArgs.SettingName, out var list))
         return;

      foreach (var item in list.Values)
         item.Handler.Invoke(sender, eArgs);
   }
}