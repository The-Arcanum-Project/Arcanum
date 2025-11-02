namespace Arcanum.Core.Utils.DelayedEvents;

using System.Windows;

public class PropertyValueChangedEventArgs(PropertyItem? changedItem, object? oldValue) : EventArgs
{
   public PropertyItem? ChangedItem { get; } = changedItem;

   public object? OldValue { get; } = oldValue;
}

public class PropGridDelayEvent : DelayedEvent<PropertyValueChangedEventArgs>
{
   private readonly Dictionary<string, object?> _firstInvocationValues = [];

   public PropGridDelayEvent(int millisecondsDelay) : base(millisecondsDelay)
   {
      Timer = new(millisecondsDelay);
      Timer.Elapsed += (_, _) => OnElapsed();
   }

   public new void Invoke(object? sender, PropertyValueChangedEventArgs args)
   {
      if (args.ChangedItem?.PropertyInfo.DeclaringType == null)
         return;

      var name = args.ChangedItem.PropertyInfo.DeclaringType.FullName ?? string.Empty;
      if (!_firstInvocationValues.TryGetValue(name, out var oldValue))
         oldValue = _firstInvocationValues[name] = args.OldValue;

      if (Equals(oldValue, args.ChangedItem.Value))
      {
         Cancel(args.ChangedItem.PropertyInfo.Name);
         return;
      }

      base.Invoke(sender, args);
   }

   private void Cancel(string key)
   {
      lock (Lock)
      {
         Timer.Stop();
         LastEventArgs = null;
         LastSender = null;
         _firstInvocationValues.Remove(key);
      }
   }

   private void OnElapsed()
   {
      object? sender;

      lock (Lock)
      {
         sender = LastSender;
         var args = LastEventArgs;

         if (args == null || Disposed || LastEventArgs == null)
            return;

         var firstOldValue = _firstInvocationValues
                            .FirstOrDefault(kvp => kvp.Key == LastEventArgs.ChangedItem?.PropertyInfo.Name)
                            .Value;

         var newArgs = new PropertyValueChangedEventArgs(LastEventArgs.ChangedItem,
                                                         firstOldValue ?? LastEventArgs.OldValue);

         var dispatcher = Application.Current?.Dispatcher;

         if (dispatcher != null)
            dispatcher.Invoke(() => CustomEventHandler?.Invoke(sender, newArgs));
         else
            // fallback if no dispatcher (e.g. in unit test)
            CustomEventHandler?.Invoke(sender, newArgs);

         _firstInvocationValues.Remove(LastEventArgs.ChangedItem?.PropertyInfo.DeclaringType?.FullName ?? string.Empty);
         LastEventArgs = null;
         LastSender = null;
      }
   }
}