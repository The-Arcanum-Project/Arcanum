using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Topography))]
public partial class TopographyParsing(IEnumerable<IDependencyNode<string>> dependencies) : ParserValidationLoadingService<Topography>(dependencies)
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.Topography,
                               lockObject);
   }
}