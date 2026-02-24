namespace Arcanum.Core.ApplicationContext;

public static class ArcAppContext
{
   private static readonly Dictionary<Type, object> ActiveContexts = new();

   public static EventHandler<object>? ContextUpdated = delegate { };

   // Ui elements recieving focus or becoming active will call this.
   public static void UpdateContext(Type interfaceType, object instance)
   {
      ActiveContexts[interfaceType] = instance;
      ContextUpdated?.Invoke(null, instance);
   }

   public static void RemoveContext(Type interfaceType)
   {
      if (ActiveContexts.Remove(interfaceType, out var instance))
         ContextUpdated?.Invoke(null, instance);
   }

   public static T? Get<T>() where T : class => ActiveContexts.TryGetValue(typeof(T), out var instance) ? (T)instance : null;

   public static bool Has<T>() where T : class => ActiveContexts.ContainsKey(typeof(T));
}