using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.Court.State;

public enum GovernmentType
{
   [EnumAgsData("monarchy")]
   Monarchy,

   [EnumAgsData("republic")]
   Republic,

   [EnumAgsData("theocracy")]
   Theocracy,

   [EnumAgsData("tribe")]
   Tribal,

   [EnumAgsData("steppe_horde")]
   SteppeHorde
}