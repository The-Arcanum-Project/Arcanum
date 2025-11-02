using System.Collections;
using System.Reflection;
using Arcanum.Core.CoreSystems.NUI;

namespace Arcanum.Core.Utils.DevHelper;

/// <summary>
/// A debug-only helper to create a sample instance of any INUI type for inspection.
/// </summary>
public static class DebugInstanceProvider
{
   public enum InstanceCreationStrategy
   {
      FromProvider,
      FromParameterlessConstructor,
      FromParameterizedConstructor,
   }

   public static bool ImplementsGenericInterface(this Type type, Type genericInterface, out Type? implementedType)
   {
      implementedType = null;
      if (!genericInterface.IsInterface || !genericInterface.IsGenericTypeDefinition)
         return false;

      implementedType = type.GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType &&
                                                 i.GetGenericTypeDefinition() == genericInterface);

      return implementedType != null;
   }

   public static bool TryGetInstance(Type type, InstanceCreationStrategy strategy, out INUI? instance)
   {
      instance = null;

      // Strategy 1: Try to get a live instance from the static ICollectionProvider.
      if (TryGetInstanceFromProvider(type, out instance))
         return true;

      if (strategy < InstanceCreationStrategy.FromParameterlessConstructor)
         return false;

      // Strategy 2: Try to use a parameterless constructor.
      if (TryGetInstanceFromParameterlessConstructor(type, out instance))
         return true;

      if (strategy < InstanceCreationStrategy.FromParameterizedConstructor)
         return false;

      // Strategy 3: Try to use the simplest parameterized constructor.
      if (TryGetInstanceFromParameterizedConstructor(type, out instance))
         return true;

      return false;
   }

   private static bool TryGetInstanceFromProvider(Type type, out INUI? instance)
   {
      instance = null;
      var methodInfo = type.GetMethod("GetGlobalItems", BindingFlags.Public | BindingFlags.Static);
      if (methodInfo == null)
         return false;

      var allItems = (IEnumerable)methodInfo.Invoke(null, null)!;
      var firstItem = allItems.Cast<object>().FirstOrDefault();

      if (firstItem is INUI nuiInstance)
      {
         instance = nuiInstance;
         return true;
      }

      return false;
   }

   private static bool TryGetInstanceFromParameterlessConstructor(Type type, out INUI? instance)
   {
      instance = null;
      if (type.GetConstructor(Type.EmptyTypes) != null)
         try
         {
            instance = Activator.CreateInstance(type) as INUI;
            return instance != null;
         }
         catch
         {
            /* The constructor might throw, which is fine, we'll just fail. */
         }

      return false;
   }

   private static bool TryGetInstanceFromParameterizedConstructor(Type type, out INUI? instance)
   {
      instance = null;
      // Find the constructor with the fewest parameters as it's the easiest to satisfy.
      var constructor = type.GetConstructors()
                            .OrderBy(c => c.GetParameters().Length)
                            .FirstOrDefault();

      if (constructor == null)
         return false;

      try
      {
         var parameters = constructor.GetParameters();
         var args = new object?[parameters.Length];

         for (var i = 0; i < parameters.Length; i++)
            // Create default/null values for the arguments.
            args[i] = parameters[i].ParameterType.IsValueType
                         ? Activator.CreateInstance(parameters[i].ParameterType)
                         : null;

         instance = constructor.Invoke(args) as INUI;
         return instance != null;
      }
      catch
      {
         /* Invoking with default args might fail, which is okay. */
      }

      return false;
   }
}