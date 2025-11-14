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
         InjectedProperties = injectSource.GetInjectedProperties(),
         Source = injectSource.Source,
         FileLocation = injectSource.FileLocation,
      };

      RegisterInjectObj(injectObj);
      return injectObj;
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
      obj.Source.ObjectsInFile.Add(obj);
   }

   public static KeyValuePair<Enum, object>[] GetInjectedProperties(this IEu5Object target)
   {
      List<KeyValuePair<Enum, object>> ips = [];

      if (!Injects.TryGetValue(target, out var injectObjs))
         return ips.ToArray();

      foreach (var injectObj in injectObjs)
         ips.AddRange(injectObj.InjectedProperties);

      return ips.ToArray();
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
   private static bool AreCollectionsLogicallyEqual(object current, object @default)
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