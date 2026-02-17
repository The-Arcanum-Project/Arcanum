using System.Globalization;
using System.Windows.Input;
using Arcanum.UI.Components.Converters;

namespace Arcanum.UI.Commands.KeyMap;

public class MultiKeyGesture : InputGesture
{
   public MultiKeyGesture(Key firstKey, ModifierKeys firstModifiers, Key secondKey, ModifierKeys secondModifiers)
   {
      FirstGesture = new(firstKey, firstModifiers);
      SecondGesture = new(secondKey, secondModifiers);
   }

   public MultiKeyGesture(KeyGesture first, KeyGesture second)
   {
      FirstGesture = first;
      SecondGesture = second;
   }

   public KeyGesture FirstGesture { get; }
   public KeyGesture SecondGesture { get; }

   // This is required by WPF, but we will handle the logic in our Binder
   // because standard WPF doesn't track "state" between key presses.
   public override bool Matches(object targetElement, InputEventArgs inputEventArgs) => false;

   public override string ToString()
   {
      var converter = new GestureToTextConverter();
      var first = converter.Convert(FirstGesture, typeof(string), null, CultureInfo.CurrentCulture);
      var second = converter.Convert(SecondGesture, typeof(string), null, CultureInfo.CurrentCulture);
      return $"{first}, {second}";
   }
}