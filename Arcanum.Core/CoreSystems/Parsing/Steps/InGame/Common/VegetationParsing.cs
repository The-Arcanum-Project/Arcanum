using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Vegetation))]
public partial class VegetationParsing : ParserValidationLoadingService<Vegetation>
{
   public override List<Type> ParsedObjects { get; } = [typeof(Vegetation)];
   public override string GetFileDataDebugInfo() => $"Parsed Vegetation: {Globals.Vegetation.Count}";

   protected override bool UnloadSingleFileContent(Eu5FileObj<Vegetation> fileObj)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Vegetation.Remove(obj.UniqueKey);

      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<Vegetation> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);
         var vegetation = new Vegetation { UniqueKey = key, Source = fileObj };

         var unhandledNodes = ParseProperties(bn, vegetation, ctx, source, ref validation);
         if (!Globals.Vegetation.TryAdd(key, vegetation))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           key,
                                           typeof(Vegetation),
                                           Vegetation.Field.UniqueKey);
         }

         foreach (var node in unhandledNodes)
         {
            if (node.IsBlockNode(ctx, source, actionStack, ref validation, out _))
               continue;

            ctx.SetPosition(node.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidBlockName,
                                           actionStack,
                                           node.KeyNode.GetLexeme(source),
                                           new[]
                                           {
                                              VegetationKeywords.COLOR, VegetationKeywords.DEBUG_COLOR,
                                              VegetationKeywords.DEFENDER, VegetationKeywords.HAS_SAND,
                                              VegetationKeywords.MOVEMENT_COST,
                                           });
         }
      }
   }
}