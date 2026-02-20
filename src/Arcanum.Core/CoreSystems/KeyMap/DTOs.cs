namespace Arcanum.Core.CoreSystems.KeyMap;

/// <summary>
///    Represents a single keyboard combination (e.g., Ctrl + S)
/// </summary>
public class ShortcutStroke(string key, string modifiers)
{
   public string Key { get; set; } = key;
   public string Modifiers { get; set; } = modifiers;
   public bool IsEmpty => string.IsNullOrEmpty(Key);

   public override string ToString() => string.IsNullOrEmpty(Modifiers) ? Key : $"{Modifiers}+{Key}";
}

/// <summary>
///    Represents a full command shortcut, supporting single keys or Chords (multi-stroke)
/// </summary>
public class ShortcutChord(ShortcutStroke firstStroke, ShortcutStroke? secondStroke = null)
{
   public ShortcutStroke FirstStroke { get; set; } = firstStroke;
   public ShortcutStroke? SecondStroke { get; set; } = secondStroke;
   public bool IsChord => SecondStroke != null;

   public override string ToString() => IsChord ? $"{FirstStroke}, {SecondStroke}" : FirstStroke.ToString();

   public override bool Equals(object? obj)
   {
      if (obj is not ShortcutChord other)
         return false;

      return FirstStroke.Key.Equals(other.FirstStroke.Key, StringComparison.OrdinalIgnoreCase) &&
             FirstStroke.Modifiers.Equals(other.FirstStroke.Modifiers, StringComparison.OrdinalIgnoreCase) &&
             ((SecondStroke == null && other.SecondStroke == null) ||
              (SecondStroke != null &&
               other.SecondStroke != null &&
               SecondStroke.Key.Equals(other.SecondStroke.Key, StringComparison.OrdinalIgnoreCase) &&
               SecondStroke.Modifiers.Equals(other.SecondStroke.Modifiers, StringComparison.OrdinalIgnoreCase)));
   }

   protected bool Equals(ShortcutChord other) => FirstStroke.Equals(other.FirstStroke) && Equals(SecondStroke, other.SecondStroke);

   // ReSharper disable twice NonReadonlyMemberInGetHashCode
   public override int GetHashCode() => HashCode.Combine(FirstStroke, SecondStroke);
}

public enum ConflictSeverity
{
   None,
   Warning, // Conflict in a different scope (Informative)
   Error, // Conflict in the same or overlapping scope (Blocker)
}

public record ConflictResult(
   ConflictSeverity Severity,
   string ConflictingCommandId,
   string ConflictingCommandName,
   string Scope
);