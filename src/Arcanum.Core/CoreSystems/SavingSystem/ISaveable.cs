using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;

namespace Arcanum.Core.CoreSystems.SavingSystem;

public interface ISaveable
{
   public FileInformation FileInformation { get; }
   public SaveableType SaveType { get; }
}