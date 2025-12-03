using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Climate))]
public partial class ClimateParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Climate>(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ref pc,
                               ParseProperties,
                               Globals.Climates,
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Climate target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}