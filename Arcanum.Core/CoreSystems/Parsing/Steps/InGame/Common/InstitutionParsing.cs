using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class InstitutionParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Institution)];
   public override string GetFileDataDebugInfo() => $"Parsed Institutions: {Globals.Institutions.Count}";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      const string ageKey = "age";
      const string actionName = $"{nameof(InstitutionParsing)}.DiscoverObjects";

      var validation = true;
      var rns = Parser.Parse(fileObj, out var source, out var ctx);

      foreach (var sn in rns.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, nameof(InstitutionParsing), ref validation, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);

         Institution religiousSchool = new(key);
         if (!Globals.Institutions.TryAdd(key, religiousSchool))
         {
            ctx.SetPosition(bn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.DuplicateObjectDefinition,
                                           actionName,
                                           key,
                                           nameof(ReligiousSchool),
                                           ReligiousSchool.Field.UniqueId);
            validation = false;
            continue;
         }

         foreach (var bnSn in bn.Children)
         {
            if (bnSn is not ContentNode cn)
               continue;

            var identifier = cn.KeyNode.GetLexeme(source);
            if (!string.Equals(identifier, ageKey, StringComparison.Ordinal))
               continue;

            if (!cn.GetString(ctx, actionName, source, ref validation, out var age))
               continue;

            religiousSchool.Age = age;
         }
      }

      return validation;
   }

   public override bool IsFullyParsed => false;

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.Institutions.Clear();
      return true;
   }
}