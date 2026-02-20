using System.Windows.Input;
using Arcanum.Core.CoreSystems.KeyMap;
using Arcanum.Core.Settings.SmallSettingsObjects;

namespace Arcanum.UI.Commands.KeyMap;

public static class ShortcutValidator
{
   private static readonly string[] ModifierKeys = ["Control", "Alt", "Shift", "Windows", "System"];

   /// <summary>
   ///    Checks if a stroke is a "Final" key (not just a modifier like 'Ctrl')
   /// </summary>
   public static bool IsValidStroke(ShortcutStroke stroke)
   {
      if (stroke.IsEmpty)
         return false;

      return !ModifierKeys.Contains(stroke.Key, StringComparer.OrdinalIgnoreCase);
   }

   /// <summary>
   ///    Core logic to determine if two scopes overlap.
   ///    "Global" overlaps with everything.
   /// </summary>
   public static bool ScopesOverlap(string scopeA, string scopeB)
   {
      if (scopeA.Equals(CommandScopes.GLOBAL, StringComparison.OrdinalIgnoreCase) ||
          scopeB.Equals(CommandScopes.GLOBAL, StringComparison.OrdinalIgnoreCase))
         return true;

      return scopeA.Equals(scopeB, StringComparison.OrdinalIgnoreCase);
   }

   /// <summary>
   ///    Scans existing profiles to find any shortcut collisions.
   /// </summary>
   public static List<ConflictResult> FindConflicts(
      ShortcutChord newShortcut,
      string targetScope,
      IEnumerable<CommandShortcut> existingProfiles,
      string currentCommandId)
   {
      var conflicts = new List<ConflictResult>();

      foreach (var profile in existingProfiles)
      {
         // Don't conflict with yourself
         if (profile.CommandId == currentCommandId)
            continue;

         foreach (var existingChord in profile.Shortcuts)
            if (newShortcut.Equals(existingChord))
            {
               var overlaps = ScopesOverlap(targetScope, profile.Scope);

               conflicts.Add(new(overlaps ? ConflictSeverity.Error : ConflictSeverity.Warning,
                                 profile.CommandId,
                                 profile.CommandDisplayName,
                                 profile.Scope));
            }
      }

      return conflicts;
   }

   public static ShortcutChord ToChord(InputGesture gesture)
   {
      if (gesture is MultiKeyGesture multi)
         return new(new(multi.FirstGesture.Key.ToString(), multi.FirstGesture.Modifiers.ToString()),
                    new(multi.SecondGesture.Key.ToString(), multi.SecondGesture.Modifiers.ToString()));

      var kg = (KeyGesture)gesture;
      return new(new(kg.Key.ToString(), kg.Modifiers.ToString()));
   }

   public static InputGesture FromChord(ShortcutChord chord)
   {
      var k1 = Enum.Parse<Key>(chord.FirstStroke.Key);
      var m1 = Enum.Parse<ModifierKeys>(chord.FirstStroke.Modifiers);

      if (chord is { IsChord: true, SecondStroke: not null })
      {
         var k2 = Enum.Parse<Key>(chord.SecondStroke.Key);
         var m2 = Enum.Parse<ModifierKeys>(chord.SecondStroke.Modifiers);
         return new MultiKeyGesture(k1, m1, k2, m2);
      }

      return new KeyGesture(k1, m1);
   }
}