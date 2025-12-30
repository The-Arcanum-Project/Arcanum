using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;

public enum ParliamentType
{
   [EnumAgsData("country")]
   Country,

   [EnumAgsData("international_organization")]
   InternationalOrganization,
}