using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Dynasty))]
public partial class DynastyManagerParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Dynasty>(dependencies)
{
   public override string[] GroupingNodeNames => ["dynasty_manager"];

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      if (!ParsingMaster.ParsingMaster.RemoveAllGroupingNodes(rn,
                                                              ref pc,
                                                              GroupingNodeNames,
                                                              out var sns))
         return;

      SimpleObjectParser.Parse(fileObj,
                               sns,
                               ref pc,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Dynasty target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}