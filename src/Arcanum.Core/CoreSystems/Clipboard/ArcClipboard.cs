using System.Diagnostics;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.Clipboard;

public static class ArcClipboard
{
   // We want to keep a history of the last N copied items in the future
   private const int MAX_HISTORY_SIZE = 10;
   private static readonly Queue<ClipboardPayload> History = new ();

   public static event Action<ClipboardPayload>? OnCopyAction;
   public static event Action<ClipboardPayload>? OnPasteAction;

   public static void Copy(Enum type, object data)
   {
      AddToHistory(new (type, data));
      ArcLog.WriteLine("ACB", LogLevel.DBG, "Copied payload of type {0} with value {1}", type.GetType().Name, data.ToString() ?? "null");
   }

   public static void Copy(IEu5Object target, Enum? property)
   {
      Debug.Assert(property != null);
      Debug.Assert(target.GetAllProperties().Contains(property));
      AddToHistory(new (property, target._getValue(property)));
      ArcLog.WriteLine("ACB",
                       LogLevel.DBG,
                       "Copied payload of type {0} with value {1}",
                       property.GetType().Name,
                       target._getValue(property).ToString() ?? "null");
   }

   public static void Copy(IEu5Object target)
   {
      AddToHistory(new (null, target));
      ArcLog.WriteLine("ACB",
                       LogLevel.DBG,
                       "Copied entire object of type {0} with UniqueId {1}",
                       target.GetType().Name,
                       target.UniqueId);
   }

   public static bool CanPaste(IEu5Object target)
   {
      if (CurrentPayload == null)
         return false;

      if (CurrentPayload.Property == null)
         return target.GetType() == CurrentPayload.Value.GetType();

      return target.GetAllProperties().Contains(CurrentPayload.Property);
   }

   public static bool CanPaste(IEu5Object target, Enum property)
   {
      if (CurrentPayload?.Property == null)
         return false;

      if (CurrentPayload.Property.Equals(property))
         return true;

      // Allow pasting if the types are compatible
      var targetPropType = target.GetNxPropType(property);
      var payloadType = CurrentPayload.Value.GetType();
      return targetPropType == payloadType || targetPropType.IsAssignableFrom(payloadType);
   }

   public static void Paste(IEu5Object target)
   {
      if (!CanPaste(target))
         return;

      Debug.Assert(CurrentPayload != null);
      Debug.Assert(CurrentPayload.Property != null);
      Nx.ForceSet(CurrentPayload.Value, target, CurrentPayload.Property);
      OnPasteAction?.Invoke(CurrentPayload);
   }

   public static void Paste(IEu5Object target, Enum property)
   {
      if (!CanPaste(target, property))
         return;

      Debug.Assert(CurrentPayload != null);
      Debug.Assert(property != null);
      Nx.ForceSet(CurrentPayload.Value, target, property);
      OnPasteAction?.Invoke(CurrentPayload);
   }

   // History management methods
   private static void AddToHistory(ClipboardPayload payload)
   {
      if (History.Count >= MAX_HISTORY_SIZE)
         History.Dequeue();

      History.Enqueue(payload);
      OnCopyAction?.Invoke(payload);
   }

   public static ClipboardPayload? CurrentPayload => History.Count > 0 ? History.Last() : null;
   public static IEnumerable<ClipboardPayload> GetHistory() => History.ToList();

   public static void ClearHistory() => History.Clear();
}