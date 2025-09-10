using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.Common;

public class ReligiousSchoolsParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(ReligiousSchool)];
   public override string GetFileDataDebugInfo() => $"Loaded '{Globals.ReligiousSchools.Count}' religious schools.";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var isFlawless = true;
      var rn = Parser.Parse(fileObj, out var source, out var ctx);

      foreach (var sn in rn.Statements)
      {
         if (!sn.IsBlockNode(ctx, nameof(ReligiousSchoolsParsing), source, ref isFlawless, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);
         if (Globals.ReligiousSchools.TryAdd(key, new(key)))
            continue;

         ctx.SetPosition(bn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.DuplicateObjectDefinition,
                                        nameof(ReligiousSchoolsParsing),
                                        key,
                                        nameof(ReligiousSchool),
                                        ReligiousSchool.Field.Name);
         isFlawless = false;
      }

      return isFlawless;
   }

   public override bool IsFullyParsed => false;

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.ReligiousSchools.Clear();
      return true;
   }
}