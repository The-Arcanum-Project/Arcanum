using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Economy.SubClasses;

public enum MaterialGatheringMethod
{
   [EnumAgsData("farming")]
   Farming,

   [EnumAgsData("mining")]
   Mining,

   [EnumAgsData("gathering")]
   Gathering,

   [EnumAgsData("hunting")]
   Hunting,

   [EnumAgsData("forestry")]
   Forestry,

   [EnumAgsData("produced")]
   Produced,
}