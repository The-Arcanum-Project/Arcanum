using Arcanum.Core.Utils.DelayedEvents;

namespace UnitTests.Utils;

[TestFixture, Apartment(ApartmentState.STA)] // STA needed for Dispatcher
public class DelayedEventTests
{
   private DelayedEvent<EventArgs> _delayedEvent;

   [SetUp]
   public void Setup()
   {
      _delayedEvent = new(100);
   }

   [TearDown]
   public void TearDown()
   {
      _delayedEvent.Dispose();
   }

   [Test]
   public void Invoke_FiresEventAfterDelay()
   {
      var triggered = false;
      var mre = new ManualResetEventSlim();

      _delayedEvent.AddHandler((_, _) =>
      {
         triggered = true;
         mre.Set();
      });

      _delayedEvent.Invoke(this, EventArgs.Empty);

      Assert.That(triggered, Is.False, "Event should not fire immediately");
      Assert.That(mre.Wait(500), "Event did not fire within timeout");
      Assert.That(triggered);
   }

   [Test]
   public void Invoke_OnlyLastEventArgsUsedWhenCalledMultipleTimesQuickly()
   {
      EventArgs? lastArgs = null;
      var mre = new ManualResetEventSlim();

      _delayedEvent = new(100);
      _delayedEvent.AddHandler((_, e) =>
      {
         lastArgs = e;
         mre.Set();
      });

      var args1 = EventArgs.Empty;
      var args2 = EventArgs.Empty;

      _delayedEvent.Invoke(this, args1);
      _delayedEvent.Invoke(this, args2); // Only this should fire

      Assert.That(mre.Wait(500));
      Assert.That(lastArgs, Is.SameAs(args2));
   }

   [Test]
   public void Cancel_PreventsEventFromFiring()
   {
      var triggered = false;
      _delayedEvent.AddHandler((_, _) => triggered = true);

      _delayedEvent.Invoke(this, EventArgs.Empty);
      _delayedEvent.Cancel();

      Thread.Sleep(200);

      Assert.That(triggered, Is.False);
   }

   [Test]
   public void Dispose_StopsTimerAndNoEventAfterDispose()
   {
      var triggered = false;
      _delayedEvent.AddHandler((_, _) => triggered = true);

      _delayedEvent.Dispose();
      _delayedEvent.Invoke(this, EventArgs.Empty);

      Thread.Sleep(200);

      Assert.That(triggered, Is.False);
   }

   [Test]
   public void AddHandlerAndRemoveHandler_WorkAsExpected()
   {
      var triggered = false;
      Action<object?, EventArgs> handler = (_, _) => triggered = true;

      _delayedEvent.AddHandler(handler);
      _delayedEvent.Invoke(this, EventArgs.Empty);
      Thread.Sleep(150);
      Assert.That(triggered);

      triggered = false;
      _delayedEvent.RemoveHandler(handler);
      _delayedEvent.Invoke(this, EventArgs.Empty);
      Thread.Sleep(150);
      Assert.That(triggered, Is.False);
   }

   [Test]
   public void Invoke_WithNullSenderAndArgs_HandledGracefully()
   {
      var invoked = false;
      _delayedEvent.AddHandler((_, _) => invoked = true);

      _delayedEvent.Invoke(null, null!); // null args, force non-null with null-forgiving
      Thread.Sleep(150);

      Assert.That(invoked, Is.False, "Event should not fire with null args");
   }
}