using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using DefaultMapDefinition = Arcanum.Core.GameObjects.InGame.Map.DefaultMapDefinition;

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
         Dispatch(sn, Globals.DefaultMapDefinition, ref pc);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   DefaultMapDefinition target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}