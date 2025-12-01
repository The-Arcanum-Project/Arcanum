using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

[ParserFor(typeof(DefaultMapDefinition))]
public partial class DefaultMapParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<DefaultMapDefinition>(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      foreach (var sn in rn.Statements)
         if (sn is BlockNode bn)
            Pdh.DispatchBlockNode(bn,
                                  Globals.DefaultMapDefinition,
                                  ref pc,
                                  _blockParsers);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   DefaultMapDefinition target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}