using System.Collections;
using System.Diagnostics;

namespace Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

public static class InjectManager
{
   public static readonly Dictionary<IEu5Object, List<InjectObj>> Injects = new();

   public static InjectObj CreateAndRegisterInjectObj(IEu5Object target, IEu5Object injectSource, InjRepType type)
   {
      var injectObj = new InjectObj
      {
         InjRepType = type,
         Target = target,
         InjectedProperties = GetInjectedProperties(target, injectSource),
         Source = injectSource.Source,
         FileLocation = injectSource.FileLocation,
      };

      RegisterInjectObj(injectObj);
      return injectObj;
   }

   public static KeyValuePair<Enum, object>[] GetInjectedProperties(IEu5Object target, IEu5Object injectSource)
   {
      // Is not as bad as it looks as GetAllProperties() is cached.
      var nonDefaultProps = new KeyValuePair<Enum, object>[target.GetAllProperties().Length];

      var index = 0;
      foreach (var prop in target.GetAllProperties())
      {
         var currentValue = target._getValue(prop);
         var injectValue = injectSource._getValue(prop);

         if (target.IsCollection(prop) &&
             (AreCollectionsLogicallyEqual(currentValue, injectValue) || (ICollection)injectValue is { Count: 0 }))
            continue;

         if (currentValue.Equals(injectValue))
            continue;

         // Skip UniqueId property
         if (prop.ToString() == "UniqueId")
            continue;

         // TODO: this can be done much better but requires changes to how parsing works. We would need a system to set 
         // TODO: which properties were explicitly set during parsing instead of relying on default value comparison.
         // If it is of a primitive type, then we assume it is injected anyhow
         // If it is a complex type, we only want to inject if it is different from the default value
         if (!target.GetNxPropType(prop).IsPrimitive)
         {
            // Yes any empty list would be ignored but then again they have to be created as a replace
            var defaultValue = target.GetDefaultValue(prop);
            if (injectValue.Equals(defaultValue))
               continue;
         }

         nonDefaultProps[index++] = new(prop, injectValue);
      }

      return nonDefaultProps[..index];
   }

   public static void UnregisterInjectObj(InjectObj obj)
   {
      if (Injects.TryGetValue(obj.Target, out var list))
      {
         list.Remove(obj);
      }
      else
      {
         Debug.Fail("Tried to unregister an InjectObj that was not registered.");
         ArcLog.WriteLine("IMN",
                          LogLevel.ERR,
                          "Tried to unregister an InjectObj that was not registered.");
      }
   }

   public static void RegisterInjectObj(InjectObj obj)
   {
      if (!Injects.TryGetValue(obj.Target, out var list))
      {
         list = [];
         Injects[obj.Target] = list;
      }

      list.Add(obj);
   }

   public static InjectObj[] GetInjectsForTarget(this IEu5Object target)
   {
      if (!Injects.TryGetValue(target, out var injectObjs))
         return [];

      return injectObjs.ToArray();
   }

   /// <summary>
   /// Compares two collections for logical equality with high performance.
   /// Handles ordered (List, Array) and unordered (HashSet) collections correctly.
   /// </summary>
   public static bool AreCollectionsLogicallyEqual(object current, object @default)
   {
      if (ReferenceEquals(current, @default))
         return true; // Both are same instance or both are null

      var currentEnum = (IEnumerable)current;
      var defaultEnum = (IEnumerable)@default;

      switch (current)
      {
         // This requires a bit of reflection to be fully generic, but we can check for a common interface.
         // This correctly compares {1, 2} and {2, 1} as equal.
         // HashSet<T> implements IStructuralEquatable. The Equals method is highly optimized.
         case IStructuralEquatable currentSet when @default.GetType() == current.GetType():
            return currentSet.Equals(@default, EqualityComparer<object>.Default);
         // For collections with a known count (List, Array, etc.)
         // Check count first. 
         case ICollection currentCol when @default is ICollection defaultCol:
         {
            if (currentCol.Count != defaultCol.Count)
               return false;

            // If both are empty, they are equal.
            if (currentCol.Count == 0)
               return true;

            break;
         }
      }

      return currentEnum.Cast<object>().SequenceEqual(defaultEnum.Cast<object>());
   }

   public static void MergeInjects(this IEu5Object target, KeyValuePair<Enum, object>[] ips)
   {
      foreach (var (key, value) in ips)
         if (target.IsCollection(key))
            target._addToCollection(key, value);
         else
            target._setValue(key, value);
   }
}