using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.Cultural.SubObjects;

public enum CharactersHaveDynasty
{
   [EnumAgsData("sometimes")]
   Sometimes,

   [EnumAgsData("always")]
   Always,

   [EnumAgsData("never")]
   Never,
}