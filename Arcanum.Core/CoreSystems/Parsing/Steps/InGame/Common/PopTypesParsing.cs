using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class PopTypeDiscoverer(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<PopType>(true, dependencies);

[ParserFor(typeof(PopType))]
public partial class PopTypesParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<PopType>(false, dependencies)
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      SimpleObjectParser.ParseDiscoveredObjectProperties(rn,
                                                         ctx,
                                                         actionStack,
                                                         source,
                                                         ref validation,
                                                         ParseProperties,
                                                         GetGlobals());
   }
}