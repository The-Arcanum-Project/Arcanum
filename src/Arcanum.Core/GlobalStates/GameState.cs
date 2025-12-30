using Arcanum.Core.GameObjects.MainMenu.States;
using ReligiousSchoolRelations = Arcanum.Core.GameObjects.InGame.Religious.SubObjects.ReligiousSchoolRelations;

namespace Arcanum.Core.GlobalStates;

public class GameState
{
   public Dictionary<string, ReligiousSchoolRelations> ReligiousSchoolRelations { get; set; } = new();
   public InstitutionManager InstitutionManager { get; set; } = new();
}