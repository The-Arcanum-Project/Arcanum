using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.Court.State.SubClasses;

public enum ParliamentType
{
   [EnumAgsData("country")]
   Country,

   [EnumAgsData("international_organization")]
   InternationalOrganization,
}