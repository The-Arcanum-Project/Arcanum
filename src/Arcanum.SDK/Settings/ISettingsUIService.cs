using Arcanum.API.Events;
using Arcanum.API.UtilServices;

namespace Arcanum.API.Settings;

/// <summary>
/// Provides an interface to manage the user interface of settings for plugins and the application.
/// </summary>
public interface ISettingsUiService : IService
{
   /// <summary>
   /// Occurs when a setting is changed within a plugin or the core system.
   /// This event allows subscribers to handle changes to settings, providing
   /// access to information such as the plugin name, setting key, and new value.
   /// </summary>
   public EventHandler<InputConfirmEventArgs> SettingChanged { get; set; }

   /// <summary>
   /// Displays the settings window for the specified plugin or the main settings window if no plugin is specified.
   /// </summary>
   /// <param name="settings"></param>
   /// <param name="focusOnGuid"></param>
   /// <param name="host"></param>
   void ShowSettingsWindow(Dictionary<Guid, IPluginSetting> settings, Guid focusOnGuid, IPluginHost host);
}