// ReSharper disable InconsistentNaming

namespace Arcanum.API.Events;

/// <summary>
/// Contains all event IDs used by plugins.
/// The IDs should be unique across all plugins.
/// The IDs are categorized by a prefix that indicates the event type.
/// </summary>
public enum PluginEventId
{
   /* ID format:
    * <Prefix>_<EventName>
    *
    * Prefixes:
    * - Load 1-200: Events that are triggered during the loading phase of Arcanum.Nexus.Core
    * - Settings 201-300: Events that are triggered when settings are changed in Arcanum.Nexus.Core
    * - Selection 301-400: Events that are triggered when a selection is made in Arcanum.Nexus.Core
    * - GUI 401-600: Events that are triggered when the GUI is interacted with in Arcanum.Nexus.Core by the user
    * - Plugin 1000-1099: Events that are triggered by plugin management in Arcanum.PluginHost
    */

   // Load events (1-200)
   Load_Pre_DataLoading = 1, // Triggered before data loading starts
   Load_After_DataLoaded = 2, // Triggered after data loading is complete

   // Settings events (201-300)
   Settings_OnSettingChanged = 201, // Triggered when a setting is changed in the application or a plugin

   // Selection events (301-400)

   // GUI events (401-600)

   // Plugin management events (1000-1099)
   Plugin_OnLoadingComplete = 1000, // Triggered when the plugin host has finished loading all plugins
   Plugin_OnPluginReloaded = 1001, // Triggered when a plugin is reloaded
}