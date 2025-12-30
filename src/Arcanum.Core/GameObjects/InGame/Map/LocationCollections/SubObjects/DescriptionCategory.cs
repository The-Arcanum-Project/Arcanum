using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects;

public enum DescriptionCategory
{
   [EnumAgsData("administrative")]
   Administrative,

   [EnumAgsData("diplomatic")]
   Diplomatic,

   [EnumAgsData("military")]
   Military,
}