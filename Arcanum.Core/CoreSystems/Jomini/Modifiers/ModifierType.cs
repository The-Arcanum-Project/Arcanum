using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.CoreSystems.Jomini.Modifiers;

/// <summary>
/// A type assigned to a modifier to determine how its value should be interpreted.
/// </summary>
public enum ModifierType
{
   [EnumAgsData("", true)]
   Integer,

   [EnumAgsData("", true)]
   Boolean,

   [EnumAgsData("", true)]
   Float,

   [EnumAgsData("", true)]
   Percentage,

   [EnumAgsData("", true)]
   ScriptedValue,
}