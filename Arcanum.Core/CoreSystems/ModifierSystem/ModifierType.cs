namespace Arcanum.Core.CoreSystems.ModifierSystem;

/// <summary>
/// A type assigned to a modifier to determine how its value should be interpreted.
/// </summary>
public enum ModifierType
{
   Integer,
   Boolean,
   Floating,
   Percentage,
   ScriptedValue,
}