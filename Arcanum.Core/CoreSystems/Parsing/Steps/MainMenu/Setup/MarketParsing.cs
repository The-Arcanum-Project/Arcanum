using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Economy;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Market))]
public static partial class MarketParsing;

public class MarketManagerParsing : ParserValidationLoadingService<Market>
{
   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      if (rn.Statements.Count != 1)
      {
         De.Warning(ctx,
                    ParsingError.Instance.InvalidBlockCount,
                    actionStack,
                    rn.Statements.Count);
         validation = false;
         return;
      }

      if (!rn.Statements[0].IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
         return;

      foreach (var sn in bn.Children)
      {
         if (!sn.IsContentNode(ctx, source, actionStack, ref validation, out var cn))
            continue;

         var market = new Market();
         Pdh.DispatchContentNode(cn, market, ctx, source, actionStack, MarketParsing._contentParsers, ref validation);
         Globals.Markets[market.UniqueId] = market;
      }
   }
}