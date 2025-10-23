namespace Arcanum.Core.Settings.BaseClasses;

public class SettingsEventArgs(string nameSpace, string settingName, object? oldValue, object? newValue, bool handled)
{
   public string NameSpace { get; } = nameSpace;
   public string SettingName { get; } = settingName;
   public object? OldValue { get; } = oldValue;
   public object? NewValue { get; } = newValue;
   public bool Handled { get; set; } = handled;
}