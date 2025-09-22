using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;

public class ColorParser : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(JominiColor)];
   public override string GetFileDataDebugInfo() => $"Parsed Colors: {ColorResolver.Instance.ColorMap.Count}";

   public override bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      const string colorKey = "colors";
      const string actionStack = nameof(ColorParser);
      var rn = Parser.Parse(fileObj, out var source, out var ctx);
      var validation = true;

      rn.HasXStatements(ctx, 1, ref validation);
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
            continue;

         if (!bn.KeyNode.GetLexeme(source).Equals(colorKey, StringComparison.Ordinal))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidBlockName,
                                           actionStack,
                                           bn.KeyNode.GetLexeme(source),
                                           colorKey);
            validation = false;
            continue;
         }

         foreach (var scn in bn.Children)
         {
            if (!scn.IsContentNode(ctx, source, actionStack, ref validation, out var cn))
               continue;
            if (!cn.HasFunctionNode(ctx, source, actionStack, ref validation, out var fn))
               continue;
            if (!fn.GetColorDefinition(ctx, source, actionStack, ref validation, out var color))
               continue;

            var colorName = scn.KeyNode.GetLexeme(source);
            if (ColorResolver.Instance.TryAddColor(colorName, color))
               continue;

            ctx.SetPosition(scn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateColorDefinition,
                                           actionStack,
                                           colorName,
                                           nameof(JominiColor));
            validation = false;
         }
      }

      return validation;
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      return true;
   }
}