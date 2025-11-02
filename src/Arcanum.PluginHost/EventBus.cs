using System.Diagnostics;
using Arcanum.API.Events;
using Arcanum.API.UtilServices;

namespace Arcanum.PluginHost;

public class EventBus : IEventBus
{
   public static EventBus Instance { get; } = new();
   private readonly Dictionary<(PluginEventId, Type), List<Delegate>> _handlers = [];
   private readonly Dictionary<(PluginEventId, Type), SortedDictionary<int, List<Delegate>>> _uiHandlers = [];

   /// <summary>
   /// Registers a handler for a specified event ID, associating it with a specific type of event arguments.
   /// Multiple handlers can be registered for the same event ID and type.
   /// </summary>
   /// <param name="id">The unique identifier of the event to register the handler for. Must adhere to predefined plugin event IDs.</param>
   /// <param name="handler">The action to be executed when the event of the specified type is triggered.</param>
   /// <typeparam name="T">The type of the event arguments expected by the handler.</typeparam>
   public void Register<T>(PluginEventId id, Action<T> handler) where T : EventArgs
   {
      var key = (id, typeof(T));
      if (!_handlers.TryGetValue(key, out var list))
      {
         list = [];
         _handlers[key] = list;
      }

      list.Add(handler);
   }

   /// <summary>
   /// Registers a UI event handler for the specified event ID with a given priority.
   /// Adds the handler to the corresponding UI handler collection.
   /// They are executed in descending order of priority, meaning higher priority handlers are executed first.
   /// </summary>
   /// <param name="id">The unique identifier of the UI event to register the handler for. Must be in the range 400-599.</param>
   /// <param name="handler">The handler to be executed when the UI event is triggered.</param>
   /// <param name="priority">The priority of the handler. Higher priority handlers are executed before lower priority ones.</param>
   /// <typeparam name="T">The type of the event arguments handled by the handler.</typeparam>
   /// <exception cref="ArgumentOutOfRangeException">Thrown if the event ID is not within the range 400-599.</exception>
   public void RegisterUiEvent<T>(PluginEventId id, Action<T> handler, int priority) where T : EventArgs
   {
      if ((int)id < 400 || (int)id > 599)
         throw new ArgumentOutOfRangeException(nameof(id),
                                               "UI events must be in the range 400-599. Are you calling the wrong method?");

      if (!_uiHandlers.TryGetValue((id, typeof(T)), out var dict))
      {
         dict = new(Comparer<int>.Create((a, b) => b.CompareTo(a))); // Descending
         _uiHandlers[(id, typeof(T))] = dict;
      }

      if (!dict.TryGetValue(priority, out var delegates))
      {
         delegates = [];
         dict[priority] = delegates;
      }

      delegates.Add(handler);
   }

   /// <summary>
   /// Unregisters a previously registered event handler for the specified event ID.
   /// Removes the handler from the corresponding event handler collection or UI handler collection.
   /// </summary>
   /// <param name="id">The unique identifier of the event to unregister the handler from.</param>
   /// <param name="handler">The handler to be removed from the specified event.</param>
   /// <typeparam name="T">The type of the event arguments handled by the handler.</typeparam>
   public void Unregister<T>(PluginEventId id, Action<T> handler) where T : EventArgs
   {
      if ((int)id < 400 || (int)id > 599)
      {
         var key = (id, typeof(T));
         if (_handlers.TryGetValue(key, out var list))
            list.Remove(handler);
      }
      else
      {
         // This might cause issues if the same handler is registered with different priorities.
         if (_uiHandlers.TryGetValue((id, typeof(T)), out var dict))
            foreach (var kvp in dict)
            {
               kvp.Value.Remove(handler);
               if (kvp.Value.Count == 0)
                  dict.Remove(kvp.Key);
            }
      }
   }

   /// <summary>
   /// Triggers an event with the specified ID and arguments, invoking all registered handlers.
   /// Can also be used to trigger UI events, which will be handled by the UI event handlers.
   /// </summary>
   /// <param name="id">The unique identifier of the event to trigger.</param>
   /// <param name="args">The arguments to pass to the event handlers.</param>
   /// <typeparam name="T">The type of the event arguments.</typeparam>
   public void Trigger<T>(PluginEventId id, T args) where T : EventArgs
   {
      var key = (id, typeof(T));
      if ((int)id < 400 || (int)id > 599) // Regular event
      {
         if (!_handlers.TryGetValue(key, out var list))
            return;

         foreach (var handler in list.OfType<Action<T>>())
            try
            {
               handler.Invoke(args);
            }
            catch (Exception ex)
            {
               Debug.WriteLine($"Event error: {ex.Message}");
            }
      }
      else
      {
         if (!_uiHandlers.TryGetValue(key, out var dict))
            return;

         foreach (var kvp in dict)
            foreach (var handler in kvp.Value)
               ((Action<T>)handler)(args);
      }
   }

   public void Unload()
   {
   }

   // The only invalid state for the event bus is when the _host or the _infoService is null.
   // However, since we throw exceptions in the constructor if they are null, and they are
   // readonly, we can safely return Ok state here.
   public IService.ServiceState VerifyState()
   {
      return IService.ServiceState.Ok;
   }
}