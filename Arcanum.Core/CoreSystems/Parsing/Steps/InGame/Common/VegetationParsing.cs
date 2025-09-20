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

   protected override bool UnloadSingleFileContent(Eu5FileObj<Vegetation> fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.Vegetation.Remove(obj.UniqueId);

      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<Vegetation> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);
         var vegetation = new Vegetation { UniqueId = key, Source = fileObj };

         ParseProperties(bn, vegetation, ctx, source, ref validation, false);

         if (lockObject != null)
         {
            lock (lockObject)
               if (!Globals.Vegetation.TryAdd(key, vegetation))
               {
                  ctx.SetPosition(bn.KeyNode);
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.DuplicateObjectDefinition,
                                                 actionStack,
                                                 key,
                                                 typeof(Vegetation),
                                                 Vegetation.Field.UniqueId);
               }
         }
         else if (!Globals.Vegetation.TryAdd(key, vegetation))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           key,
                                           typeof(Vegetation),
                                           Vegetation.Field.UniqueId);
         }
      }
   }
}