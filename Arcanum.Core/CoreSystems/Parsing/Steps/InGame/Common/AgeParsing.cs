using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.AbstractMechanics;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Age))]
public partial class AgeParsing : ParserValidationLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Age)];
   public override string GetFileDataDebugInfo() => $"Parsed Ages: {Globals.Ages.Count}";

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      return true;
   }

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

         var ageName = bn.KeyNode.GetLexeme(source);
         if (Globals.Ages.Any(x => x.Name.Equals(ageName)))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx,
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           ageName,
                                           typeof(Age),
                                           Age.Field.Name);
            validation = false;
            continue;
         }

         var age = new Age(ageName);
         Globals.Ages.Add(age);
         var unknownNodes = ParseProperties(bn, age, ctx, source, ref validation);

         foreach (var ukn in unknownNodes)
            ukn.IsBlockNode(ctx, source, actionStack, out _);
      }
   }
}