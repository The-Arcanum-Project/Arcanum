namespace Arcanum.Core.CoreSystems.Clipboard;

/// <summary>
/// If Null we are copying an entire object rather than a property
/// </summary>
public class ClipboardPayload(Enum? property, object value)
{
   public Enum? Property { get; } = property;
   public object Value { get; } = value;
}