namespace Arcanum.Core.CoreSystems.KeyMap;

/// <summary>
///    Represents a single keyboard combination (e.g., Ctrl + S)
/// </summary>
public record ShortcutStroke(string Key, string Modifiers)
{
   public bool IsEmpty => string.IsNullOrEmpty(Key);

   public override string ToString() => string.IsNullOrEmpty(Modifiers) ? Key : $"{Modifiers}+{Key}";
}

/// <summary>
///    Represents a full command shortcut, supporting single keys or Chords (multi-stroke)
/// </summary>
public record ShortcutChord(ShortcutStroke FirstStroke, ShortcutStroke? SecondStroke = null)
{
   public bool IsChord => SecondStroke != null;

   public override string ToString() => IsChord ? $"{FirstStroke}, {SecondStroke}" : FirstStroke.ToString();
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