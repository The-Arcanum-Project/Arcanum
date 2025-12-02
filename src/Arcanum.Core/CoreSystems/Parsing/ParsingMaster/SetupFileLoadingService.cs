using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class SetupFileLoadingService(IEnumerable<IDependencyNode<string>> dependencies)
   : GeneralFileLoadingService(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      throw new NotSupportedException("Setup files are not loaded via this method.");
   }

   public abstract void LoadSetupFile(StatementNode sn,
                                      ref ParsingContext pc,
                                      Eu5FileObj fileObj,
                                      object? lockObject);
}