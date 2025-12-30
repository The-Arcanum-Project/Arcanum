using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.InGame.CountryLevel;

public enum CountryType
{
   [EnumAgsData("army")]
   Army,

   [EnumAgsData("pop")]
   Pop,

   [EnumAgsData("building")]
   Building,

   [EnumAgsData("location")]
   Location,
}