using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;

public enum TraitFlavorType
{
   [EnumAgsData("", true)]
   None,

   [EnumAgsData("personality")]
   Personality,

   [EnumAgsData("government_approach")]
   GovernmentApproach,

   [EnumAgsData("education")]
   Education,

   [EnumAgsData("interests")]
   Interests,
}