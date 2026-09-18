#region

using Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

#endregion

namespace Arcanum.Core.GameObjects.InGame.Court.State.SubClasses;

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

   [EnumAgsData("cabinet")]
   Cabinet,

   [EnumAgsData("health")]
   Health,
}

public enum JuvenileForm
{
   [EnumAgsData("none", true)]
   None,

   [EnumAgsData("eunuch")]
   Eunuch,
}