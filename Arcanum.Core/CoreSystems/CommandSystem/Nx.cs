using System.Diagnostics;
using System.Reflection;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.CommandSystem;

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
      CommandManager.SetValueCommand((IEu5Object)target, e, value!);
      target._setValue(e, value!);
   }

   /// <summary>
   /// ONLY USE THIS IF YOU WANT TO TRIGGER UI UPDATES
   /// </summary>
   /// <param name="value"></param>
   /// <param name="target"></param>
   /// <param name="e"></param>
   /// <typeparam name="T"></typeparam>
   public static void ForceSet<T>(T value,
                                  INexus target,
                                  Enum e)
   {
      CommandManager.SetValueCommand((IEu5Object)target, e, value!);
      target._setValue(e, value!);
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

   public static T ForceGetAs<T>(
      INexus target,
      Enum e)
   {
      return (T)target._getValue(e);
   }

   public static Type TypeOf(INexus _,
                             [LinkedPropertyEnum(nameof(_))] Enum e)
   {
      return e.GetType()
              .GetField(e.ToString())
              .GetCustomAttribute<ExpectedTypeAttribute>()
              .Type; // TODO @Melco replace this with sourgenerated lookup as this is slow af
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

   public static void AddToCollection<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      T value)
   {
      Debug.Assert(value != null, nameof(value) + " != null");
      CommandManager.AddToCollectionCommand((IEu5Object)target, e, value);
      target._addToCollection(e, value!);
   }

   public static void RemoveFromCollection<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      T value)
   {
      Debug.Assert(value != null, nameof(value) + " != null");
      CommandManager.RemoveFromCollectionCommand((IEu5Object)target, e, value);
      target._removeFromCollection(e, value!);
   }

   /// <summary>
   /// TRIGGERS UI UPDATES AND COMMAND CREATION
   /// </summary>
   public static void ClearCollection(INexus target,
                                      [LinkedPropertyEnum(nameof(target))] Enum e)
   {
      CommandManager.ClearCollectionCommand((IEu5Object)target, e);
      target._clearCollection(e);
   }

   /// <summary>
   /// Returns all Nexus Properties of a specific type.
   /// </summary>
   /// <param name="target"></param>
   /// <param name="type"></param>
   /// <returns></returns>
   public static Enum[] GetPropertiesOfType(INexus target, Type type)
   {
      return target.GetAllProperties().Where(prop => target.GetNxPropType(prop) == type).ToArray();
   }

   public static Enum[] GetGraphableProperties(INexus target)
   {
      return GetPropertiesOfType(target, target.GetType());
   }
}