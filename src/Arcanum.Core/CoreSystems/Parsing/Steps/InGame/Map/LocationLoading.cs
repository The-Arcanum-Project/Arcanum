using System.Globalization;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class LocationFileLoading(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Location>(dependencies)
{
   public override void LoadSingleFile(RootNode rn,
                                       LocationContext ctx,
                                       Eu5FileObj fileObj,
                                       string actionStack,
                                       string source,
                                       ref bool validation,
                                       object? lockObject)
   {
      var cIndex = 0;
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsContentNode(ctx, source, actionStack, ref validation, out var cn))
            continue;

         if (!cn.Value.IsLiteralValueNode(ctx, source, ref validation, out var lvn))
            continue;

         if (!int.TryParse(lvn.Value.GetLexeme(source),
                           NumberStyles.HexNumber,
                           CultureInfo.InvariantCulture,
                           out var hex))
         {
            ctx.SetPosition(lvn);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.HexToIntConversionError,
                                           actionStack,
                                           lvn.Value.GetLexeme(source));
            validation = false;
            continue;
         }

         var key = cn.KeyNode.GetLexeme(source);
         var loc = IEu5Object<Location>.CreateInstance(key, fileObj);
         loc.Color = new JominiColor.Int(hex);

         if (!cn.KeyNode.IsSimpleKeyNode(ctx, source, actionStack, out var skn))
            continue;

         if (LUtil.TryAddToGlobals(ctx, skn.KeyToken, key, actionStack, ref validation, loc))
            loc.ColorIndex = cIndex++;
      }
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Location target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("LocationFileLoading should only be used in discovery phase.");

   public override bool CanBeReloaded => false;
}