using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public interface IAgs : INexus
{
   public AgsSettings Settings { get; }
}