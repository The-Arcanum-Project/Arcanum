using Arcanum.Core.CoreSystems.SavingSystem.AGS;

namespace Arcanum.Core.Settings;

// ReSharper disable once InconsistentNaming
public class AGSSettings
{
   public AgsSettings ModifierDataSettings { get; set; } = new();
   public AgsSettings AgeAgsSettings { get; set; } = new();
   public AgsSettings VegetationAgsSettings { get; set; } = new();
   public AgsSettings TopographyAgsSettings { get; set; } = new();
   public AgsSettings ClimateAgsSettings { get; set; } = new();
   public AgsSettings RoadAgsSettings { get; set; } = new();
}