using System.Windows.Input;
using Arcanum.API.Core.KeyBinds;

namespace UnitTests.CoreSystems.Common.KeyBinds;

public class TestKeyBindProvider : KeyBindProvider
{
   public KeyGesture Save { get; set; } = new(Key.S, ModifierKeys.Control);
   public KeyGesture Open { get; set; } = new(Key.O, ModifierKeys.Control);
}

[TestFixture]
public class KeyBindProviderTests
{
   [Test]
   public void GetKeyBinds_Returns_All_KeyGestures()
   {
      var provider = new TestKeyBindProvider();
      var keyBinds = provider.GetKeyBinds();

      Assert.That(keyBinds, Has.Count.EqualTo(2));
      Assert.That(keyBinds, Does.ContainKey(nameof(TestKeyBindProvider.Save)));
      Assert.That(keyBinds[nameof(TestKeyBindProvider.Open)].Key, Is.EqualTo(Key.O));
   }

   [Test]
   public void SetKeyBind_Updates_Specified_Bind()
   {
      var provider = new TestKeyBindProvider();
      var newGesture = new KeyGesture(Key.K, ModifierKeys.Alt);

      provider.SetKeyBind(nameof(TestKeyBindProvider.Save), newGesture);

      var updated = provider.GetKeyBinds()[nameof(TestKeyBindProvider.Save)];
      Assert.That(updated.Key, Is.EqualTo(Key.K));
      Assert.That(updated.Modifiers, Is.EqualTo(ModifierKeys.Alt));
   }

   [Test]
   public void ResetKeyBind_Reverts_To_Default()
   {
      var provider = new TestKeyBindProvider();
      provider.SetKeyBind(nameof(TestKeyBindProvider.Save), new(Key.K, ModifierKeys.Control));

      provider.ResetKeyBind(nameof(TestKeyBindProvider.Save));

      var reset = provider.GetKeyBinds()[nameof(TestKeyBindProvider.Save)];
      Assert.That(reset.Key, Is.EqualTo(Key.S));
      Assert.That(reset.Modifiers, Is.EqualTo(ModifierKeys.Control));
   }

   [Test]
   public void ResetAllKeyBinds_Reverts_All_To_Defaults()
   {
      var provider = new TestKeyBindProvider();
      provider.SetKeyBind(nameof(TestKeyBindProvider.Save), new(Key.X, ModifierKeys.Alt));
      provider.SetKeyBind(nameof(TestKeyBindProvider.Open), new(Key.Y, ModifierKeys.Alt));

      provider.ResetAllKeyBinds();
      var keyBinds = provider.GetKeyBinds();

      Assert.That(keyBinds[nameof(TestKeyBindProvider.Save)].Key, Is.EqualTo(Key.S));
      Assert.That(keyBinds[nameof(TestKeyBindProvider.Open)].Key, Is.EqualTo(Key.O));
   }

   [Test]
   public void ResetKeyBind_InvalidName_Throws()
   {
      var provider = new TestKeyBindProvider();

      var ex = Assert.Throws<KeyNotFoundException>(() =>
                                                      provider.ResetKeyBind("Nonexistent"));

      Assert.That(ex.Message, Does.Contain("Key bind 'Nonexistent' not found"));
   }

   [Test]
   public void ResetAllKeyBinds_MissingParameterlessConstructor_Throws()
   {
      var provider = new NoDefaultCtorKeyBindProvider("dummy");

      var ex = Assert.Throws<InvalidOperationException>(() =>
                                                           provider.ResetAllKeyBinds());

      Assert.That(ex.Message, Does.Contain("parameterless constructor"));
   }

   private class NoDefaultCtorKeyBindProvider : KeyBindProvider
   {
      // ReSharper disable once UnusedParameter.Local
      public NoDefaultCtorKeyBindProvider(string dummy)
      {
         // This class intentionally has no parameterless constructor
      }

      // ReSharper disable once UnusedMember.Local
      public KeyGesture Hidden { get; set; } = new(Key.H, ModifierKeys.Alt);

      public override void SetKeyBinds(Dictionary<string, KeyGesture> keyBinds)
      {
      }
   }
}