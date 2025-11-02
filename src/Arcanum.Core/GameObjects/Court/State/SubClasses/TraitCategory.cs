using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

namespace Arcanum.Core.GameObjects.Court.State.SubClasses;

public enum TraitCategory
{
   [EnumAgsData("ruler")]
   Ruler,

   [EnumAgsData("general")]
   General,

   [EnumAgsData("admiral")]
   Admiral,

   [EnumAgsData("artist")]
   Artist,

   [EnumAgsData("explorer")]
   Explorer,

   [EnumAgsData("religious_figure")]
   ReligiousFigure,

   [EnumAgsData("child")]
   Child,
}