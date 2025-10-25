using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.Economy.SubClasses;

public enum MaterialCategory
{
   [EnumAgsData("produced")]
   Produced,

   [EnumAgsData("raw_material")]
   RawMaterial,
}