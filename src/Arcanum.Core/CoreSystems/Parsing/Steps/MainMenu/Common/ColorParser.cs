using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;

public class ColorParser(IEnumerable<IDependencyNode<string>> dependencies) : FileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects { get; } = [typeof(JominiColor)];
   public override string GetFileDataDebugInfo() => $"Parsed Colors: {ColorResolver.Instance.ColorMap.Count}";

   public override bool LoadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      const string colorKey = "colors";
      const string actionStack = nameof(ColorParser);
      var rn = Parser.Parse(fileObj, out var source, out var lctx);
      var validation = true;
      var pc = new ParsingContext(lctx, source.AsSpan(), actionStack, ref validation);
      using var ctx = pc.PushScope();

      rn.HasXStatements(ref pc, 1);
      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ref pc, out var bn))
            continue;

         var key = pc.SliceString(bn);
         if (!key.Equals(colorKey, StringComparison.Ordinal))
         {
            pc.SetContext(bn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidBlockName,
                                           key,
                                           colorKey);
            pc.Fail();
            continue;
         }

         foreach (var scn in bn.Children)
         {
            if (!scn.IsContentNode(ref pc, out var cn))
               continue;
            if (!cn.HasFunctionNode(ref pc, out var fn))
               continue;
            if (!fn.GetColorDefinition(ref pc, out var color))
               continue;

            var colorName = pc.SliceString(scn);
            if (ColorResolver.Instance.TryAddColor(colorName, color))
               continue;

            pc.SetContext(scn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.DuplicateColorDefinition,
                                           colorName,
                                           nameof(JominiColor));
            pc.Fail();
         }
      }

      return validation;
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      return true;
   }

   public override bool CanBeReloaded => false;

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
   }
}