using System.ComponentModel;
using System.Reflection;
using Arcanum.API;
using Arcanum.API.Core.IO;
using Arcanum.API.Events;
using Arcanum.API.Settings;
using Arcanum.API.UtilServices;

namespace Arcanum.PluginHost.Settings;

internal class SettingsStore : ISettingsStore
{
   private readonly Dictionary<Guid, IPluginSetting> _settings = new();

   public LoggingVerbosity Verbosity { get; set; } = LoggingVerbosity.Info;
   public ISettingsUiService SettingsUi { get; }
   public event EventHandler<PluginSettingEventArgs>? SettingChanged;
   private readonly IPluginHost _host;

   public SettingsStore(ISettingsUiService uiService, IPluginHost host)
   {
      _host = host ?? throw new ArgumentNullException(nameof(host), "Plugin host cannot be null.");
      SettingsUi = uiService ?? throw new ArgumentNullException(nameof(uiService), "UI service cannot be null.");

      // We check if the required services are registered.
      // If the services are not registered yet, we get an exception.
      _host.GetService<IFileOperations>();
      _host.GetService<IJsonProcessor>();

      Load();
   }

   public bool Set(Guid guid, IPluginSetting value)
   {
      _settings[guid] = value ?? throw new ArgumentNullException(nameof(value), "Plugin setting cannot be null.");

      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      RaiseEvent(value, value, null);
      return true;
   }

   /// <summary>
   /// Updates the value of a specified property within a setting identified by a GUID.
   /// </summary>
   /// <typeparam name="T">The type of the value to be set.</typeparam>
   /// <param name="propertyInfo">The metadata information of the property to update.</param>
   /// <param name="guid">The unique identifier of the setting object containing the property.</param>
   /// <param name="value">The new value to set for the specified property.</param>
   /// <returns>
   /// A boolean value indicating whether the update was successful. Returns true if the value was updated, false if no update occurred.
   /// </returns>
   /// <exception cref="ArgumentException">Thrown if the provided GUID is empty.</exception>
   public bool Set<T>(PropertyInfo propertyInfo, Guid guid, T value)
   {
      if (propertyInfo == null! || value == null)
      {
         Log("Neither PropertyInfo nor value can be null.", LoggingVerbosity.Error);
         return false;
      }

      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      if (!_settings.TryGetValue(guid, out var setting))
      {
         Log($"Setting with GUID {guid} not found.", LoggingVerbosity.Warning);
         return false;
      }

      var currentValue = propertyInfo.GetValue(setting);
      if (currentValue == null)
      {
         Log($"Current value for property '{propertyInfo.Name}' is null. Cannot set to new value.",
             LoggingVerbosity.Warning);
         return false; // No current value to compare against
      }

      if (Equals(value, currentValue))
         return false; // No change

      propertyInfo.SetValue(setting, value);
      RaiseEvent(setting, value, propertyInfo);
      return true;
   }

   /// <summary>
   /// Updates the specified property within the settings identified by a GUID with a new value.
   /// </summary>
   /// <typeparam name="T">The type of the value to be set.</typeparam>
   /// <param name="propertyName">The name of the property to be updated.</param>
   /// <param name="guid">The unique identifier of the settings object containing the property.</param>
   /// <param name="value">The new value to set for the specified property.</param>
   /// <returns>
   /// A boolean value indicating whether the update was successful. Returns true if the value
   /// has been updated, false if no update occurred or if the property was not found.
   /// </returns>
   public bool Set<T>(string propertyName, Guid guid, T value)
   {
      if (string.IsNullOrWhiteSpace(propertyName) || value == null)
      {
         Log("Neither propertyName nor value can be null or empty.", LoggingVerbosity.Error);
         return false;
      }

      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      var propertyInfo = _settings[guid].GetType().GetProperty(propertyName);
      if (propertyInfo == null)
      {
         Log($"Property '{propertyName}' not found in setting with GUID {guid}.", LoggingVerbosity.Error);
         return false;
      }

      return Set(propertyInfo, guid, value);
   }

   /// <summary>
   /// Checks if a setting contains a property with the specified name, identified by the provided GUID.
   /// </summary>
   /// <param name="propertyName">The name of the property to check.</param>
   /// <param name="guid">The unique identifier of the setting object to search in.</param>
   /// <returns>
   /// A boolean value indicating whether the specified property exists within the setting. Returns true if the property is found, otherwise false.
   /// </returns>
   /// <exception cref="ArgumentNullException">Thrown if the provided property name is null, empty, or consists only of whitespace.</exception>
   /// <exception cref="ArgumentException">Thrown if the provided GUID is empty.</exception>
   public bool Contains(string propertyName, Guid guid)
   {
      if (string.IsNullOrWhiteSpace(propertyName))
         throw new ArgumentNullException(nameof(propertyName), "PropertyInfo cannot be null.");

      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      if (!_settings.TryGetValue(guid, out var setting))
      {
         Log($"Setting with GUID {guid} not found.", LoggingVerbosity.Warning);
         return false;
      }

      return setting.GetType().GetProperty(propertyName) != null;
   }

   /// <summary>
   /// Determines whether a specified property exists within a setting identified by a GUID.
   /// </summary>
   /// <param name="propertyInfo"></param>
   /// <param name="guid">The unique identifier of the setting object containing the property.</param>
   /// <returns>
   /// A boolean value indicating whether the property exists. Returns true if the property exists, false otherwise.
   /// </returns>
   public bool Contains(PropertyInfo propertyInfo, Guid guid) => Contains(propertyInfo.Name, guid);

   public bool TryGet<T>(string propertyName, Guid guid, T value)
   {
      if (string.IsNullOrWhiteSpace(propertyName))
      {
         Log("Property name cannot be null or empty.", LoggingVerbosity.Error);
         return false;
      }

      if (!_settings.TryGetValue(guid, out var setting))
      {
         Log($"Setting with GUID {guid} not found.", LoggingVerbosity.Warning);
         return false;
      }

      var propertyInfo = setting.GetType().GetProperty(propertyName);
      return TryGet(propertyInfo!, guid, out value);
   }

   public bool TryGet<T>(PropertyInfo propertyInfo, Guid guid, out T value)
   {
      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      value = default!; // Initialize value to default
      if (propertyInfo == null!)
      {
         Log($"Property '{propertyInfo}' not found in setting with GUID {guid}.", LoggingVerbosity.Error);
         return false;
      }

      if (!propertyInfo.CanRead)
      {
         Log($"Property '{propertyInfo.Name}' in setting with GUID {guid} is not readable.", LoggingVerbosity.Error);
         return false;
      }

      if (!_settings.TryGetValue(guid, out var setting))
      {
         Log($"Setting with GUID {guid} not found.", LoggingVerbosity.Warning);
         return false;
      }

      var outValue = propertyInfo.GetValue(setting);
      if (outValue == null)
      {
         Log($"Property '{propertyInfo.Name}' in setting with GUID {guid} is null.", LoggingVerbosity.Warning);
         return false; // No value to return
      }

      if (outValue is T typedValue)
      {
         value = typedValue;
         return true; // Successfully retrieved the value
      }

      return false; // Value is not of the expected type
   }

   public T Get<T>(string propertyName, IPlugin plugin)
   {
      return Get<T>(plugin.GetType().GetProperty(propertyName) ??
                    throw new ArgumentException($"Property '{propertyName}' not found in plugin '{plugin.Name}'.",
                                                nameof(propertyName)),
                    plugin);
   }

   public T Get<T>(PropertyInfo propertyInfo, IPlugin plugin)
   {
      if (propertyInfo == null!)
         throw new ArgumentNullException(nameof(propertyInfo), "PropertyInfo cannot be null.");

      if (plugin == null!)
         throw new ArgumentNullException(nameof(plugin), "Plugin cannot be null.");

      if (plugin.Guid == Guid.Empty)
         throw new ArgumentException("Plugin GUID cannot be empty.", nameof(plugin));

      if (!_settings.TryGetValue(plugin.Guid, out var setting))
      {
         Log($"Setting with GUID {plugin.Guid} not found.", LoggingVerbosity.Warning);
         return default!;
      }

      if (!propertyInfo.CanRead)
      {
         Log($"Property '{propertyInfo.Name}' in setting with GUID {plugin.Guid} is not readable.",
             LoggingVerbosity.Error);
         return default!;
      }

      var value = propertyInfo.GetValue(setting);
      if (value == null)
      {
         Log($"Property '{propertyInfo.Name}' in setting with GUID {plugin.Guid} is null.", LoggingVerbosity.Warning);
         return default!; // No value to return
      }

      return (T)value; // Cast to the expected type and return
   }

   public IPluginSetting? GetSetting(Guid guid)
   {
      return guid == Guid.Empty
                ? throw new ArgumentException("Guid cannot be empty.", nameof(guid))
                : _settings.GetValueOrDefault(guid);
   }

   public void Reset(PropertyInfo propertyInfo, Guid guid)
   {
      if (propertyInfo == null!)
         throw new ArgumentNullException(nameof(propertyInfo), "PropertyInfo cannot be null.");

      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      if (!_settings.TryGetValue(guid, out var setting))
      {
         Log($"Setting with GUID {guid} not found.", LoggingVerbosity.Warning);
         return;
      }

      GetDefaultValueAttribute(propertyInfo, out var defaultValue);
      if (defaultValue == null)
      {
         Log($"No default value found for property '{propertyInfo.Name}' in setting with GUID {guid}. Null is not valid!",
             LoggingVerbosity.Warning);
         return;
      }

      propertyInfo.SetValue(setting, defaultValue);
      RaiseEvent(setting, defaultValue, propertyInfo);
   }

   public void Reset(string propertyName, Guid guid)
   {
      if (string.IsNullOrWhiteSpace(propertyName))
         throw new ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");

      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      var propertyInfo = _settings[guid].GetType().GetProperty(propertyName);
      if (propertyInfo == null)
      {
         Log($"Property '{propertyName}' not found in setting with GUID {guid}.", LoggingVerbosity.Error);
         return;
      }

      Reset(propertyInfo, guid);
   }

   public void ResetAll(Guid guid)
   {
      if (guid == Guid.Empty)
         throw new ArgumentException("Guid cannot be empty.", nameof(guid));

      if (!_settings.TryGetValue(guid, out var setting))
      {
         Log($"Setting with GUID {guid} not found.", LoggingVerbosity.Warning);
         return;
      }

      foreach (var propertyInfo in setting.GetType().GetProperties())
      {
         GetDefaultValueAttribute(propertyInfo, out var defaultValue);
         if (defaultValue == null)
         {
            Log($"No default value found for property '{propertyInfo.Name}' in setting with GUID {guid}. Null is not valid!",
                LoggingVerbosity.Warning);
            continue;
         }

         propertyInfo.SetValue(setting, defaultValue);
         RaiseEvent(setting, defaultValue, propertyInfo);
      }
   }

   private const string PLUGIN_SETTINGS_FILE = "PluginSettings.json";

   public bool Load()
   {
      var ioService = _host.GetService<IFileOperations>();
      var jsonProcessor = _host.GetService<IJsonProcessor>();

      if (jsonProcessor == null || ioService == null)
         throw new InvalidOperationException("Required services are not available: IJsonProcessor or IFileOperations.");

      var path = Path.Combine(ioService.GetArcanumDataPath, PLUGIN_SETTINGS_FILE);

      if (!ioService.FileExists(path))
      {
         Log($"Settings file '{PLUGIN_SETTINGS_FILE}' not found at path '{path}'.", LoggingVerbosity.Warning);
         return false;
      }

      var jsonContent = ioService.ReadAllTextUtf8(path);
      if (string.IsNullOrWhiteSpace(jsonContent))
      {
         Log($"Settings file '{PLUGIN_SETTINGS_FILE}' is empty or contains only whitespace.", LoggingVerbosity.Warning);
         return false;
      }

      var settings = jsonProcessor.Deserialize<Dictionary<Guid, IPluginSetting>>(jsonContent);
      if (settings == null)
      {
         Log($"Failed to deserialize settings from file '{PLUGIN_SETTINGS_FILE}'.", LoggingVerbosity.Error);
         return false;
      }

      _settings.Clear();
      foreach (var kvp in settings)
         _settings[kvp.Key] = kvp.Value;
      return true;
   }

   public bool Save()
   {
      // TODO: Implement a proper way to serialize settings which are interfaces without creating a security risk.
      return false; // For now until we have a proper way to serialize settings which are interfaces
      var ioService = _host.GetService<IFileOperations>();
      var jsonProcessor = _host.GetService<IJsonProcessor>();

      if (jsonProcessor == null || ioService == null)
         throw new InvalidOperationException("Required services are not available: IJsonProcessor or IFileOperations.");

      var path = Path.Combine(ioService.GetArcanumDataPath, PLUGIN_SETTINGS_FILE);
      var settingsToSave = _settings.ToDictionary(pair => pair.Key,
                                                  pair => (pair.GetType(), (object)pair.Value));
      var jsonContent = jsonProcessor.Serialize(settingsToSave);
      if (string.IsNullOrWhiteSpace(jsonContent))
      {
         Log($"Failed to serialize settings to JSON for file '{PLUGIN_SETTINGS_FILE}'.", LoggingVerbosity.Error);
         return false;
      }

      if (!ioService.WriteAllTextUtf8(path, jsonContent))
      {
         Log($"Failed to write settings to file '{PLUGIN_SETTINGS_FILE}' at path '{path}'.", LoggingVerbosity.Error);
         return false;
      }

      return true;
   }

   private void RaiseEvent(IPluginSetting setting,
                           object value,
                           PropertyInfo? info,
                           EventSource source = EventSource.Plugin)
   {
      SettingChanged?.Invoke(null, new(setting.OwnerGuid, source, info, value));
   }

   public void ShowSettingsWindow(Guid focusOnPlugin) => SettingsUi.ShowSettingsWindow(_settings, focusOnPlugin, _host);

   private const string PLUGIN_SETTINGS_GROUP = "PlugSett";

   public void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
   {
      if (verbosity < Verbosity)
         return;

      _host.Log(PLUGIN_SETTINGS_GROUP, message, verbosity);
   }

   private void GetDefaultValueAttribute(PropertyInfo propertyInfo, out object? defaultValue)
   {
      var attribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
      if (attribute != null)
      {
         defaultValue = attribute.Value;
         return;
      }

      // If no DefaultValueAttribute is found, use the default value for the type
      defaultValue = propertyInfo.PropertyType.IsValueType ? Activator.CreateInstance(propertyInfo.PropertyType) : null;
   }

   public void Unload()
   {
      Save();
   }

   // State is only an error if either the settings store, the host, or the UI service is not available.
   // All of them are either throwing exceptions or readonly and default initialized in the constructor,
   // so we can safely return Ok state here.
   public IService.ServiceState VerifyState() => IService.ServiceState.Ok;
}