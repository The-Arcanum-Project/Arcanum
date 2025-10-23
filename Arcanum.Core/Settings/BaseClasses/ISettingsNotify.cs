namespace Arcanum.Core.Settings.BaseClasses;

public interface ISettingsNotify
{
   protected void SettingChanged(string key, object? oldValue, object? newValue);
}