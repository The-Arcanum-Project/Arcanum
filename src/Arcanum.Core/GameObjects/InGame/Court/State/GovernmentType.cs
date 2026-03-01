using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Court.State;

public enum GovernmentType
{
   [EnumAgsData("none", isIgnoredInSerialization: true)]
   None,

   [EnumAgsData("monarchy")]
   Monarchy,

   [EnumAgsData("republic")]
   Republic,

   [EnumAgsData("theocracy")]
   Theocracy,

   [EnumAgsData("tribe")]
   Tribal,

   [EnumAgsData("steppe_horde")]
   SteppeHorde,
}