using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

[ParserFor(typeof(DefaultMapDefinition))]
public partial class DefaultMapParsing : ParserValidationLoadingService<DefaultMapDefinition>
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      foreach (var sn in rn.Statements)
         if (sn is BlockNode bn)
            Pdh.DispatchBlockNode(bn,
                                  Globals.DefaultMapDefinition,
                                  ctx,
                                  source,
                                  actionStack,
                                  _blockParsers,
                                  ref validation);
   }
}