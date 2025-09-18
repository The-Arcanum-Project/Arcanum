using Arcanum.Core.CoreSystems.SavingSystem.AGS;

namespace Arcanum.Core.Settings;

// ReSharper disable once InconsistentNaming
public class AGSSettings
{
   public AgsSettings ModifierDataSettings { get; set; } = new();

   public AgsSettings AgeAgsSettings { get; set; } = new();
}