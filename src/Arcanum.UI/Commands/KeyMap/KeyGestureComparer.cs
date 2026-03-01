using System.Windows.Input;

namespace Arcanum.UI.Commands.KeyMap;

public class KeyGestureComparer : IEqualityComparer<InputGesture>
{
   public bool Equals(InputGesture? x, InputGesture? y)
   {
      if (x == null || y == null)
         return false;

      if (x is KeyGesture kgX && y is KeyGesture kgY)
         return kgX.Key == kgY.Key && kgX.Modifiers == kgY.Modifiers;
      if (x is MultiKeyGesture mkX && y is MultiKeyGesture mkY)
         return mkX.Equals(mkY);

      return false;
   }

   public int GetHashCode(InputGesture obj) => obj switch
   {
      KeyGesture kg => HashCode.Combine(kg.Key, kg.Modifiers),
      MultiKeyGesture mk => HashCode.Combine(mk.FirstGesture.Key, mk.FirstGesture.Modifiers, mk.SecondGesture.Key, mk.SecondGesture.Modifiers),
      _ => obj.GetHashCode(),
   };
}