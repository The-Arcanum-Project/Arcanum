using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace Arcanum.Core.Utils.DelayedEvents;

public class DelayedEvent<T> : IDisposable where T : EventArgs
{
   protected bool Disposed;

   protected Timer Timer;
   protected readonly object Lock = new();

   protected T? LastEventArgs;
   protected object? LastSender;

   protected Action<object?, T>? CustomEventHandler;

   public event Action<object?, T>? EventHandler
   {
      add => CustomEventHandler += value;
      remove => CustomEventHandler -= value;
   }

   public DelayedEvent(int millisecondsDelay)
   {
      Timer = new(millisecondsDelay);
      Timer.Elapsed += (_, _) => OnElapsed();
   }

   public void AddHandler(Action<object?, T> handler)
   {
      EventHandler += handler;
   }

   public void RemoveHandler(Action<object?, T> handler)
   {
      EventHandler -= handler;
   }

   public void Invoke(object? sender, T args)
   {
      lock (Lock)
      {
         if (Disposed)
            return;

         LastEventArgs = args;
         LastSender = sender;

         Timer.Stop();
         Timer.Start();
      }
   }

   public void Cancel()
   {
      lock (Lock)
      {
         Timer.Stop();
         LastEventArgs = null;
         LastSender = null;
      }
   }

   private void OnElapsed()
   {
      object? sender;
      T? args;

      lock (Lock)
      {
         sender = LastSender;
         args = LastEventArgs;
         LastEventArgs = null;
         LastSender = null;
      }

      if (args != null)
      {
         var dispatcher = Application.Current?.Dispatcher;

         if (dispatcher != null)
            dispatcher.Invoke(() => CustomEventHandler?.Invoke(sender, args));
         else
            // fallback if no dispatcher (e.g. in unit test)
            CustomEventHandler?.Invoke(sender, args);
      }
   }

   public void Dispose()
   {
      lock (Lock)
      {
         if (Disposed)
            return;

         Timer.Dispose();
         Disposed = true;
      }

      GC.SuppressFinalize(this);
   }
}