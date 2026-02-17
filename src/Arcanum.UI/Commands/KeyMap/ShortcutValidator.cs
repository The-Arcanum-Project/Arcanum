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
      IEnumerable<CommandShortcutProfile> existingProfiles,
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
}