using Arcanum.API.UtilServices;

namespace Arcanum.API.Events;

/// <summary>
/// Defines the interface for an event bus that facilitates event registration, unregistration,
/// and triggering functionality for events with specific IDs.
/// </summary>
public interface IEventBus : IService
{
   /// <summary>
   /// Registers an event handler for a specific plugin event ID.
   /// </summary>
   /// <typeparam name="T">The type of event arguments associated with the event.</typeparam>
   /// <param name="id">The ID of the event to register the handler for.</param>
   /// <param name="handler">The action to execute when the event is triggered.</param>
   void Register<T>(PluginEventId id, Action<T> handler) where T : EventArgs;

   /// <summary>
   /// Registers an ui event handler for a specific plugin event ID with an optional priority level.
   /// Event handlers registered with this method will be executed in the order of their priority,
   /// where the higher the priority value, the earlier the handler is executed.
   /// </summary>
   /// <typeparam name="T">The type of event arguments associated with the event.</typeparam>
   /// <param name="id">The ID of the event to register the handler for.</param>
   /// <param name="handler">The action to execute when the event is triggered.</param>
   /// <param name="priority">The priority level of the handler, determining the order of
   /// execution. Higher values indicate higher priority. Defaults to 0.</param>
   public void RegisterUiEvent<T>(PluginEventId id, Action<T> handler, int priority = 0) where T : EventArgs;

   /// <summary>
   /// Unregisters an event handler for a specific plugin event ID.
   /// </summary>
   /// <typeparam name="T">The type of event arguments associated with the event.</typeparam>
   /// <param name="id">The ID of the event to unregister the handler from.</param>
   /// <param name="handler">The action previously registered to the event that should be removed.</param>
   void Unregister<T>(PluginEventId id, Action<T> handler) where T : EventArgs;

   /// <summary>
   /// Triggers an event associated with the specified plugin event ID and provides event arguments.
   /// Is only used by the plugin host to trigger events.
   /// </summary>
   /// <typeparam name="T">The type of event arguments associated with the event.</typeparam>
   /// <param name="id">The ID of the event to be triggered.</param>
   /// <param name="args">The event arguments to pass to the event handler.</param>
   public void Trigger<T>(PluginEventId id, T args) where T : EventArgs;
}