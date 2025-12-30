using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Market = Arcanum.Core.GameObjects.InGame.Economy.Market;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Market))]
public static partial class MarketParsing;

public class MarketManagerParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Market>(dependencies)
{
   public override bool CanBeReloaded => false;

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Market target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("MarketManagerParsing does not support parsing properties to object.");

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      using var scope = pc.PushScope();
      if (rn.Statements.Count != 1)
      {
         De.Warning(ref pc,
                    ParsingError.Instance.InvalidBlockCount,
                    rn.Statements.Count);
         pc.Fail();
         return;
      }

      if (!rn.Statements[0].IsBlockNode(ref pc, out var bn))
         return;

      foreach (var sn in bn.Children)
      {
         if (!sn.IsContentNode(ref pc, out var cn))
            continue;

         var market = new Market();
         MarketParsing.Dispatch(cn, market, ref pc);
         Globals.Markets[market.UniqueId] = market;
      }
   }
}