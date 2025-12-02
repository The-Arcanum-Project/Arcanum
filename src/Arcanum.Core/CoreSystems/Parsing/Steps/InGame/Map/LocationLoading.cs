using System.Globalization;
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
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      var cIndex = 0;
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsContentNode(ref pc, out var cn))
            continue;

         if (!cn.Value.IsLiteralValueNode(ref pc, out var lvn))
            continue;

         if (!int.TryParse(pc.SliceString(lvn),
                           NumberStyles.HexNumber,
                           CultureInfo.InvariantCulture,
                           out var hex))
         {
            pc.SetContext(lvn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.HexToIntConversionError,
                                           pc.SliceString(lvn));
            pc.Fail();
            continue;
         }

         var key = pc.SliceString(cn);
         var loc = IEu5Object<Location>.CreateInstance(key, fileObj);
         loc.Color = new JominiColor.Int(hex);

         if (!cn.KeyNode.IsSimpleKeyNode(ref pc, out var skn))
            continue;

         if (LUtil.TryAddToGlobals(skn.KeyToken, ref pc, loc))
            loc.ColorIndex = cIndex++;
      }
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Location target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("LocationFileLoading should only be used in discovery phase.");

   public override bool CanBeReloaded => false;
}