using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Language))]
public partial class LanguageParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Language)];
   public override string GetFileDataDebugInfo() => $"Parsed Languages: {Globals.Languages.Count}";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      const string actionStack = nameof(LanguageParsing);
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;

      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         var langKey = bn.KeyNode.GetLexeme(source);
         var language = new Language(langKey);

         ParseProperties(bn, language, ctx, source, ref validation, false);
         if (!Globals.Languages.TryAdd(langKey, language))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionStack,
                                           langKey,
                                           typeof(Language),
                                           Language.Field.Name);
         }
      }

      return validation;
   }

   private static void ParseDialects(BlockNode dialectsBlock,
                                     Language parentLanguage,
                                     string source,
                                     LocationContext ctx,
                                     ref bool validation)
   {
      foreach (var statement in dialectsBlock.Children)
         if (statement is BlockNode dbn)
         {
            var dialect = new Language(dbn.KeyNode.GetLexeme(source));
            ParseProperties(dbn, dialect, ctx, source, ref validation, false);
            parentLanguage.Dialects.Add(dialect);
         }
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
      => throw new NotImplementedException();
}