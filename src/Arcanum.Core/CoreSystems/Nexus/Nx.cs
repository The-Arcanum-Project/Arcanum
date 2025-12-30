using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.Nexus.AggregateLinkHandling;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Utils.DataStructures;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Nexus;

public static class Nx
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   internal static int GetEnumIndex(Enum property)
   {
      Debug.Assert(Enum.GetUnderlyingType(property.GetType()) == typeof(int),
                   "Enum underlying type is not int");
      return Unsafe.Unbox<int>(property);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   internal static int GetEnumIndexByte(Enum property)
   {
      Debug.Assert(Enum.GetUnderlyingType(property.GetType()) == typeof(byte),
                   "Enum underlying type is not byte");
      return Unsafe.Unbox<byte>(property);
   }

   /// <summary>
   /// A generic setter helper. The analyzer identifies it as a setter because
   /// one of its parameters has the [PropertyValue] attribute.
   /// </summary>
   public static void Set<T>(
      IEu5Object target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      [PropertyValue] T value)
   {
      if (!target.IgnoreCommand(e))
         CommandManager.SetValueCommand(target, e, value!);
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
      if (target.IsAggregateLink(e) && NxAggregateLinkManager.ForceSet((IEu5Object)value!, (IEu5Object)target, e))
         return;
      if (!target.IgnoreCommand(e))
         CommandManager.SetValueCommand((IEu5Object)target, e, value!);
      else
         target._setValue(e, value!);
   }

   public static void ForceSet<T>(T value, IEu5Object[] targets, Enum e)
   {
      Debug.Assert(targets.Length > 0, nameof(targets) + " == 0");
      if (targets[0].IsAggregateLink(e) && NxAggregateLinkManager.ForceSet((IEu5Object)value!, targets, e))
         return;
      if (targets.Length == 1)
         ForceSet(value, targets[0], e);
      else if (!targets[0].IgnoreCommand(e))
         CommandManager.SetValueCommand(targets, e, value!);
      else
         foreach (var target in targets)
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
              .GetField(e.ToString())!
              .GetCustomAttribute<ExpectedTypeAttribute>()!
              .Type; // TODO @Melco replace this with source generated lookup as this is slow af
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

      // Check that the value is not already in the collection
      var currentCollection = (IEnumerable<T>)target._getValue(e);
      if (currentCollection.Contains(value))
         return;

      if (!target.IgnoreCommand(e))
      {
         var type = target.GetNxPropType(e);
         if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AggregateLink<>) && value is IEu5Object obj)
            CommandManager.TransferBetweenLinksCommand((IEu5Object)target, e, obj);
         else
            CommandManager.AddToCollectionCommand((IEu5Object)target, e, value);
      }
      else
         target._addToCollection(e, value);
   }

   public static void AddRangeToCollection<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      IEnumerable<T> values)
   {
      Debug.Assert(values != null, nameof(values) + " != null");

      // get only the values which are not already in the collection
      var currentCollection = (IEnumerable<T>)target._getValue(e);
      values = values.Where(v => !currentCollection.Contains(v)).ToList();

      if (!values.HasItems())
         return;

      var type = target.GetNxPropType(e);
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AggregateLink<>))
      {
         CommandManager.TransferBetweenLinksCommand((IEu5Object)target, e, values.Cast<IEu5Object>());
         return;
      }

      foreach (var value in values)
      {
         Debug.Assert(value != null, nameof(value) + " != null");
         if (!target.IgnoreCommand(e))
            CommandManager.AddToCollectionCommand((IEu5Object)target, e, value);
         else
            target._addToCollection(e, value);
      }
   }

   public static void RemoveRangeFromCollection<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      IEnumerable<T> values)
   {
      Debug.Assert(values != null, nameof(values) + " != null");

      // GET only the values which are actually in the collection
      var currentCollection = (IEnumerable<T>)target._getValue(e);
      values = values.Where(v => currentCollection.Contains(v)).ToList();

      if (!values.HasItems())
         return;

      var type = target.GetNxPropType(e);
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AggregateLink<>) && target is IEu5Object eu5Val)
      {
         CommandManager.RemoveFromLinkCommand(eu5Val, e, values.Cast<IEu5Object>());
         return;
      }

      foreach (var value in values)
      {
         Debug.Assert(value != null, nameof(value) + " != null");
         if (!target.IgnoreCommand(e))
            CommandManager.RemoveFromCollectionCommand((IEu5Object)target, e, value);
         else
            target._removeFromCollection(e, value);
      }
   }

   public static void RemoveFromCollection<T>(
      INexus target,
      [LinkedPropertyEnum(nameof(target))] Enum e,
      T value)
   {
      // Check that the value is actually in the collection before trying to remove it
      var currentCollection = (IEnumerable<T>)target._getValue(e);
      if (!currentCollection.Contains(value))
         return;

      Debug.Assert(value != null, nameof(value) + " != null");

      var type = target.GetNxPropType(e);
      if (type.IsGenericType &&
          type.GetGenericTypeDefinition() == typeof(AggregateLink<>) &&
          target is IEu5Object eu5Target &&
          value is IEu5Object eu5Val)
      {
         CommandManager.RemoveFromLinkCommand(eu5Target, e, eu5Val);
         return;
      }

      if (!target.IgnoreCommand(e))
         CommandManager.RemoveFromCollectionCommand((IEu5Object)target, e, value);
      else
         target._removeFromCollection(e, value);
   }

   /// <summary>
   /// TRIGGERS UI UPDATES AND COMMAND CREATION
   /// </summary>
   public static void ClearCollection(INexus target,
                                      [LinkedPropertyEnum(nameof(target))] Enum e)
   {
      if (!target.IgnoreCommand(e))
         CommandManager.ClearCollectionCommand((IEu5Object)target, e);
      else
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