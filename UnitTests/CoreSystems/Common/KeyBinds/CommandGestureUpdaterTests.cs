using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Arcanum.Core.Globals.BackingClasses.WindowKeyBinds;

namespace UnitTests.CoreSystems.Common.KeyBinds;

[TestFixture]
public class CommandGestureUpdaterTests
{
   private RoutedCommand _testCommand;
   private KeyGesture _keyGesture;
   private Window _window;
   private MenuItem _menuItemWithCommand;
   private MenuItem _menuItemWithoutCommand;
   
   [Apartment(ApartmentState.STA)]
   [SetUp]
   public void SetUp()
   {
      _testCommand = new();
      _keyGesture = new(Key.S, ModifierKeys.Control);
      _testCommand.InputGestures.Add(_keyGesture);

      _menuItemWithCommand = new() { Command = _testCommand };
      _menuItemWithoutCommand = new() { Command = ApplicationCommands.Copy };

      var menu = new Menu();
      menu.Items.Add(_menuItemWithCommand);
      menu.Items.Add(_menuItemWithoutCommand);

      _window = new() { Content = menu };
      _window.Show();
   }

   [Apartment(ApartmentState.STA)]
   [TearDown]
   public void TearDown()
   {
      _window.Close();
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void UpdateGestureTextInMenuItems_ValidGesture_SetsInputGestureText()
   {
      CommandGestureUpdater.UpdateGestureTextInMenuItems(_window, _testCommand);

      var expectedText = _keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
      Assert.That(_menuItemWithCommand.InputGestureText, Is.EqualTo(expectedText));
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void UpdateGestureTextInMenuItems_NoMatchingCommand_DoesNotChangeOtherMenuItems()
   {
      var originalText = _menuItemWithoutCommand.InputGestureText;

      CommandGestureUpdater.UpdateGestureTextInMenuItems(_window, _testCommand);

      Assert.That(_menuItemWithoutCommand.InputGestureText, Is.EqualTo(originalText));
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void UpdateGestureTextInMenuItems_CommandWithNoGestures_DoesNothing()
   {
      var command = new RoutedCommand();

      _menuItemWithCommand.Command = command;
      _menuItemWithCommand.InputGestureText = "original";

      CommandGestureUpdater.UpdateGestureTextInMenuItems(_window, command);

      Assert.That(_menuItemWithCommand.InputGestureText, Is.EqualTo("original"));
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void UpdateGestureTextInMenuItems_NullWindow_DoesNothing()
   {
      Assert.DoesNotThrow(() =>
                             CommandGestureUpdater.UpdateGestureTextInMenuItems(null, _testCommand));
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void UpdateGestureTextInMenuItems_NullCommand_DoesNotThrow()
   {
      Assert.DoesNotThrow(() =>
                             CommandGestureUpdater.UpdateGestureTextInMenuItems(_window, null!));
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void FindVisualChildren_WithNestedItems_FindsAllMatchingChildren()
   {
      var parent = new StackPanel();
      var child1 = new MenuItem();
      var child2 = new MenuItem();
      parent.Children.Add(new Grid { Children = { child1 } });
      parent.Children.Add(child2);

      var results = CommandGestureUpdaterTestAccessor.FindVisualChildren<MenuItem>(parent).ToList();

      Assert.That(results, Has.Count.EqualTo(2));
      Assert.That(results, Does.Contain(child1));
      Assert.That(results, Does.Contain(child2));
   }

   [Apartment(ApartmentState.STA)]
   [Test]
   public void FindVisualChildren_NullInput_YieldsNothing()
   {
      var results = CommandGestureUpdaterTestAccessor.FindVisualChildren<MenuItem>(null!);
      Assert.That(results, Is.Empty);
   }
}

internal static class CommandGestureUpdaterTestAccessor
{
   public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
      => (typeof(CommandGestureUpdater)
         .GetMethod("FindVisualChildren",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
         .MakeGenericMethod(typeof(T))
         .Invoke(null, [depObj]) as IEnumerable<T>)!;
}