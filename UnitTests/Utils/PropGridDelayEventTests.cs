using Arcanum.Core.Utils.DelayedEvents;

namespace UnitTests.Utils;

[TestFixture]
public class PropGridDelayEventTests
{
   private PropGridDelayEvent _event;
   private List<(object? sender, PropertyValueChangedEventArgs args)> _calls;

   [SetUp]
   public void Setup()
   {
      _calls = [];
      _event = new(50);
      _event.AddHandler((s, a) => _calls.Add((s, a)));
   }

   [TearDown]
   public void Teardown() => _event.Dispose();

   [Test]
   public void Should_FireEvent_AfterDelay()
   {
      var item = CreateItem("TestProp", "val1");

      _event.Invoke(this, new(item, "val0"));
      Thread.Sleep(100);

      Assert.That(_calls.Count, Is.EqualTo(1));
      Assert.That(_calls[0].args.OldValue, Is.EqualTo("val0"));
   }

   [Test]
   public void Should_NotFire_IfValueUnchanged()
   {
      var item = CreateItem("TestProp", "same");

      _event.Invoke(this, new(item, "same"));
      Thread.Sleep(100);

      Assert.That(_calls, Is.Empty);
   }

   // [Test]
   // public void Should_ResetDelay_OnMultipleInvokes()
   // {
   //    var item = CreateItem("TestProp", "step1");
   //
   //    _event.Invoke(this, new(item, "original"));
   //    Thread.Sleep(30);
   //    _event.Invoke(this, new(item, "original"));
   //    Thread.Sleep(60);
   //
   //    Assert.That(_calls, Has.Count.EqualTo(1));
   // }

   [Test]
   public void Should_TrackOriginalOldValue()
   {
      var item = CreateItem("TestProp", "val2");

      _event.Invoke(this, new(item, "original"));
      Thread.Sleep(600);
      _event.Invoke(this, new(item, "val2"));
      Thread.Sleep(80);

      Assert.That(_calls.Count, Is.EqualTo(1));
      Assert.That(_calls[0].args.OldValue, Is.EqualTo("original"));
   }

   [Test]
   public void Should_CancelOnRevertToOriginalValue()
   {
      _calls.Clear();
      var item = CreateItem("TestProp", "start");

      _event.Invoke(this, new(item, "start"));
      Thread.Sleep(30);
      _event.Invoke(this, new(item, "start"));
      Thread.Sleep(100);

      Assert.That(_calls, Is.Empty);
   }

   [Test]
   public void Dispose_ShouldPreventFiring()
   {
      var item = CreateItem("TestProp", "abc");

      _event.Invoke(this, new(item, "xyz"));
      _event.Dispose();

      Thread.Sleep(100);
      Assert.That(_calls, Is.Empty);
   }

   private static PropertyItem CreateItem(string propName, object? value)
   {
      var propInfo = typeof(Dummy).GetProperty(nameof(Dummy.Value))!;
      return new(propInfo, typeof(object), () => value, v => { value = v; }, "TestCategory");
   }

   private class Dummy
   {
      public object? Value { get; set; }
   }
}