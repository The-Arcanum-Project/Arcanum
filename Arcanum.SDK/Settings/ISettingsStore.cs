using System.Reflection;
using Arcanum.API.Events;
using Arcanum.API.UtilServices;

namespace Arcanum.API.Settings;

public interface ISettingsStore : ISubroutineLogger, IService
{
   ISettingsUiService SettingsUi { get; }
   public LoggingVerbosity Verbosity { get; set; }

   /// <summary>
   /// Updates the entire plugin settings object with a new completely new value.
   /// </summary>
   /// <param name="guid"></param>
   /// <param name="value">The new value to assign to the plugin setting.</param>
   /// <returns>
   /// True if the setting was successfully updated; otherwise, false.
   /// </returns>
   bool Set(Guid guid, IPluginSetting value);

   /// <summary>
   /// Updates the value of a specified property within a plugin's settings object.
   /// </summary>
   /// <typeparam name="T">The type of the value to be set for the property.</typeparam>
   /// <param name="propertyInfo">The metadata of the property being updated.</param>
   /// <param name="guid">The unique identifier of the plugin associated with the setting.</param>
   /// <param name="value">The new value to assign to the property.</param>
   /// <returns>
   /// True if the property was successfully updated; otherwise, false.
   /// </returns>
   bool Set<T>(PropertyInfo propertyInfo, Guid guid, T value);

   /// <summary>
   /// Updates the value of a plugin setting based on the specified property name and identifier.
   /// </summary>
   /// <typeparam name="T">The type of the value to be set for the setting.</typeparam>
   /// <param name="propertyName">The name of the property to update within the plugin setting.</param>
   /// <param name="guid">The unique identifier of the plugin to which the setting belongs.</param>
   /// <param name="value">The new value to assign to the specified plugin setting.</param>
   /// <returns>
   /// True if the setting was successfully updated; otherwise, false.
   /// </returns>
   bool Set<T>(string propertyName, Guid guid, T value);

   /// <summary>
   /// Determines whether a specified setting exists for the given plugin.
   /// </summary>
   /// <param name="propertyName">The unique propertyName identifying the setting to look for.</param>
   /// <param name="guid"></param>
   /// <returns>
   /// True if the setting exists; otherwise, false.
   /// </returns>
   bool Contains(string propertyName, Guid guid);

   /// <summary>
   /// Determines whether a setting associated with the given property and plugin exists.
   /// </summary>
   /// <param name="propertyInfo">The metadata for the property to check for the setting.</param>
   /// <param name="guid"></param>
   /// <returns>
   /// True if the specified setting exists; otherwise, false.
   /// </returns>
   public bool Contains(PropertyInfo propertyInfo, Guid guid);

   /// <summary>
   /// Attempts to retrieve the value of a setting associated with the specified property and identifier.
   /// </summary>
   /// <typeparam name="T">The type of the value to be retrieved.</typeparam>
   /// <param name="propertyName">The name of the property whose value is to be retrieved.</param>
   /// <param name="guid">The unique identifier associated with the plugin instance.</param>
   /// <param name="value">The default value to return if the setting does not exist or cannot be retrieved.</param>
   /// <returns>
   /// The value of the setting if it exists and can be retrieved; otherwise, the provided default value.
   /// </returns>
   bool TryGet<T>(string propertyName, Guid guid, T value = default!);

   /// <summary>
   /// Attempts to retrieve a setting value for a given property and identifier, returning a default value if not found.
   /// </summary>
   /// <typeparam name="T">The type of the setting to be retrieved.</typeparam>
   /// <param name="propertyInfo"></param>
   /// <param name="guid">The unique identifier for the setting.</param>
   /// <param name="value"></param>
   /// <returns>
   /// The setting value if found; otherwise, the specified default value.
   /// </returns>
   bool TryGet<T>(PropertyInfo propertyInfo, Guid guid, out T value);

   /// <summary>
   /// Retrieves the value of a plugin setting by its property name and associated plugin.
   /// </summary>
   /// <typeparam name="T">The type of the value to be retrieved.</typeparam>
   /// <param name="propertyName">The name of the property corresponding to the setting.</param>
   /// <param name="plugin">The plugin instance associated with the requested setting.</param>
   /// <returns>
   /// The value of the requested setting if found; otherwise, the specified default value.
   /// </returns>
   T Get<T>(string propertyName, IPlugin plugin);

   /// <summary>
   /// Retrieves the specified setting value associated with the plugin. If the setting does not exist, the provided default value is returned.
   /// </summary>
   /// <typeparam name="T">The type of the setting value to retrieve.</typeparam>
   /// <param name="propertyInfo">A PropertyInfo object representing the property of the setting to retrieve.</param>
   /// <param name="plugin">The plugin instance associated with the setting.</param>
   /// <returns>
   /// The value of the specified setting if found; otherwise, the provided default value.
   /// </returns>
   T Get<T>(PropertyInfo propertyInfo, IPlugin plugin);

   /// <summary>
   /// Retrieves the plugin setting associated with the specified GUID.
   /// </summary>
   /// <param name="guid">The unique identifier of the plugin setting to retrieve.</param>
   /// <returns>
   /// An instance of <see cref="IPluginSetting"/> if the setting is found; otherwise, null.
   /// </returns>
   IPluginSetting? GetSetting(Guid guid);

   /// <summary>
   /// Resets the specified setting for a plugin using the provided property information and unique identifier.
   /// </summary>
   /// <param name="propertyInfo">The property information associated with the setting to reset.</param>
   /// <param name="guid">The unique identifier of the plugin whose setting is being reset.</param>
   void Reset(PropertyInfo propertyInfo, Guid guid);

   /// <summary>
   /// Resets the specified plugin setting identified by the property name and plugin GUID to its default value.
   /// </summary>
   /// <param name="propertyName">The name of the property to reset.</param>
   /// <param name="guid">The unique identifier of the plugin owning the property to reset.</param>
   void Reset(string propertyName, Guid guid);

   /// <summary>
   /// Resets all settings associated with the specified GUID to their default values.
   /// </summary>
   /// <param name="guid">The unique identifier representing the settings to reset.</param>
   void ResetAll(Guid guid);

   /// <summary>
   /// Loads the persisted plugin settings into memory.
   /// </summary>
   /// <returns>
   /// True if the settings were successfully loaded; otherwise, false.
   /// </returns>
   bool Load();

   /// <summary>
   /// Saves the current state of the settings to a persistent storage.
   /// </summary>
   /// <returns>
   /// True if the settings were successfully saved; otherwise, false.
   /// </returns>
   bool Save();

   /// <summary>
   /// Displays the settings window for managing plugin settings.
   /// </summary>
   void ShowSettingsWindow(Guid focusOnPlugin);

   // settings events
   event EventHandler<PluginSettingEventArgs> SettingChanged;
}