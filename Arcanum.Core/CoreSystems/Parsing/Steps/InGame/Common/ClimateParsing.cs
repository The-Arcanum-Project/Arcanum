using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Climate))]
public partial class ClimateParsing : ParserValidationLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Climate)];
   public override string GetFileDataDebugInfo() => $"Parsed Climates: {Globals.Climates.Count}";

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.Climates.Clear();
      return true;
   }

   public override bool IsFullyParsed => false;

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var climateKey = bn.KeyNode.GetLexeme(source);
         var climate = new Climate(climateKey);

         var unhandledNodes = ParseProperties(bn, climate, ctx, source, ref validation);
         if (!Globals.Climates.TryAdd(climateKey, climate))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           climateKey,
                                           typeof(Climate),
                                           Climate.Field.Name);
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
                                              ClimateKeywords.COLOR, ClimateKeywords.DEBUG_COLOR,
                                              ClimateKeywords.HAS_PRECIPITATION, ClimateKeywords.WINTER,
                                           });
         }
      }
   }
}