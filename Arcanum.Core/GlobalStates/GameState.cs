using Arcanum.Core.GameObjects.MainMenu.States;
using Arcanum.Core.GameObjects.Religion.SubObjects;

namespace Arcanum.Core.GlobalStates;

public class GameState
{
   public Dictionary<string, ReligiousSchoolRelations> ReligiousSchoolRelations { get; set; } = new();
   public InstitutionManager InstitutionManager { get; set; } = new();
}