using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

public abstract class SetupFileLoadingService(IEnumerable<IDependencyNode<string>> dependencies)
   : GeneralFileLoadingService(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       LocationContext ctx,
                                       Eu5FileObj fileObj,
                                       string actionStack,
                                       string source,
                                       ref bool validation,
                                       object? lockObject)
   {
      throw new NotSupportedException("Setup files are not loaded via this method.");
   }

   public abstract bool LoadSetupFile(StatementNode sn,
                                      LocationContext ctx,
                                      Eu5FileObj fileObj,
                                      string actionStack,
                                      string source,
                                      ref bool validation,
                                      object? lockObject);
}