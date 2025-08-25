using System.Reflection;

namespace Nexus.Core;

public static class Nx
{
   /// <summary>
   /// A generic setter helper. The analyzer identifies it as a setter because
   /// one of its parameters has the [PropertyValue] attribute.
   /// </summary>
   public static void Set<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      [PropertyValue] T value)
   {
      target._setValue(e, value);
   }

   [PropertyGetter]
   public static T Get<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e)
   {
      //Console.WriteLine($"Getting generic value for {e} from {target.GetType().Name}");
      return (T)target._getValue(e);
   }

   [PropertyGetter]
   public static void Get<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      ref T returnValue)
   {
      //Console.WriteLine($"Getting generic value for {e} from {target.GetType().Name}");
      returnValue = (T)target._getValue(e);
   }

   public static void ForceGet<T>(
      INexus target,
      Enum e,
      ref T returnValue)
   {
      //Console.WriteLine($"Getting generic value for {e} from {target.GetType().Name}");
      returnValue = (T)target._getValue(e);
   }

   public static Type TypeOf(INexus _,
                             [LinkedPropertyEnum(nameof(_))] Enum e)
   {
      return e.GetType().GetField(e.ToString()).GetCustomAttribute<ExpectedTypeAttribute>().Type;
   }

   /// <summary>
   /// Returns the expected type of the item in a collection property.
   /// </summary>
   /// <returns></returns>
   public static Type TypeOfItem(INexus _,
                                 [LinkedPropertyEnum(nameof(_))] Enum e)
   {
      var collectionType = TypeOf(_, e);

      if (collectionType.IsArray)
         return collectionType.GetElementType()!;

      if (!collectionType.IsGenericType)
         return collectionType;

      var genericArgs = collectionType.GetGenericArguments();
      return genericArgs.Length == 1 ? genericArgs[0] : collectionType;
   }
}