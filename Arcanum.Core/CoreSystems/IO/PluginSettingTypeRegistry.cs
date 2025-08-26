using System.Collections.Concurrent;
using System.Reflection;
using Arcanum.API.Settings;

namespace Arcanum.Core.CoreSystems.IO;

public static class PluginSettingTypeRegistry
{
   private static readonly ConcurrentDictionary<string, Type> SKeyToTypeMap = new();
   private static readonly ConcurrentDictionary<Type, string> STypeToKeyMap = new();
   private static bool _sIsInitialized;
   private static readonly object SLock = new();

   /// <summary>
   /// Discovers and registers all IPluginSetting types from the specified assemblies.
   /// This should be called once at application startup.
   /// </summary>
   public static void Initialize(params Assembly[] assembliesToScan)
   {
      lock (SLock)
      {
         if (_sIsInitialized)
            // Or throw an exception, depending on desired behavior
            return;

         var settingTypes = assembliesToScan
                           .SelectMany(assembly => assembly.GetTypes())
                           .Where(type => typeof(IPluginSetting).IsAssignableFrom(type) &&
                                          !type.IsInterface &&
                                          !type.IsAbstract);

         foreach (var type in settingTypes)
         {
            var attribute = type.GetCustomAttribute<PluginSettingKeyAttribute>();
            if (attribute != null)
            {
               if (!SKeyToTypeMap.TryAdd(attribute.Key, type))
                  throw new
                     InvalidOperationException($"Duplicate plugin setting key '{attribute.Key}' detected on type '{type.FullName}'.");

               STypeToKeyMap.TryAdd(type, attribute.Key);
               Console.WriteLine($"Registered plugin setting: '{attribute.Key}' -> {type.FullName}");
            }
         }

         _sIsInitialized = true;
      }
   }

   public static Type? ResolveType(string key)
   {
      SKeyToTypeMap.TryGetValue(key, out var type);
      return type;
   }

   public static string? ResolveKey(Type type)
   {
      STypeToKeyMap.TryGetValue(type, out var key);
      return key;
   }
}