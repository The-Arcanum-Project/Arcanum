using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class InstitutionAndReligiousSchoolsParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(ReligiousSchool), typeof(Institution)];

   private const string RELIGION_MANAGER_KEY = "religion_manager";
   private const string INSTITUTION_MANAGER_KEY = "institution_manager";
   private const string INSTITUTIONS_KEY = "institutions";

   public override string GetFileDataDebugInfo()
      => $"Parsed Objects:\n Religious Schools: {Globals.ReligiousSchools.Count}\n Institutions: {Globals.Institutions.Count}";

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var validation = true;
      var rns = Parser.Parse(fileObj, out var source, out var ctx);

      if (!rns.HasXStatements(ctx, 2, ref validation))
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnsuccessfulFileParse,
                                        $"{nameof(InstitutionAndReligiousSchoolsParsing)}.StatementsNode.HasXStatements",
                                        fileObj.Path.FullPath);
         return false;
      }

      foreach (var sn in rns.Statements)
      {
         if (!sn.IsBlockNode(ctx, source, nameof(InstitutionAndReligiousSchoolsParsing), ref validation, out var bn))
            continue;

         var key = bn.KeyNode.GetLexeme(source);
         switch (key)
         {
            case RELIGION_MANAGER_KEY:
               ParseReligionManager(bn, ctx, source, ref validation);
               break;
            case INSTITUTION_MANAGER_KEY:
               ParseInstitutionManager(bn, ctx, source, ref validation);
               break;
            default:
               ctx.SetPosition(bn.KeyNode);
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.InvalidBlockName,
                                              nameof(InstitutionAndReligiousSchoolsParsing),
                                              key,
                                              $"{RELIGION_MANAGER_KEY}' or '{INSTITUTION_MANAGER_KEY}'");
               validation = false;
               break;
         }
      }

      return validation;
   }

   private static void ParseInstitutionManager(BlockNode bn, LocationContext ctx, string source, ref bool validation)
   {
      const string actionName = $"{nameof(InstitutionAndReligiousSchoolsParsing)}.{nameof(ParseInstitutionManager)}";
      const string activeKey = "active";
      const string birthPlaceKey = "birth_place";

      if (!bn.HasOnlyXBlocksAsChildren(ctx,
                                       source,
                                       1,
                                       actionName,
                                       ref validation,
                                       out var bns))
         return;

      var institutionsBlock = bns[0];
      if (!string.Equals(institutionsBlock.KeyNode.GetLexeme(source), INSTITUTIONS_KEY, StringComparison.Ordinal))
      {
         ctx.SetPosition(institutionsBlock.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockName,
                                        actionName,
                                        institutionsBlock.KeyNode.GetLexeme(source),
                                        INSTITUTIONS_KEY);
         validation = false;
         return;
      }

      foreach (var isn in institutionsBlock.Children)
      {
         if (!isn.IsBlockNode(ctx,
                              source,
                              actionName,
                              ref validation,
                              out var ibn))
            continue;

         if (!Globals.Institutions.TryGetValue(ibn.KeyNode.GetLexeme(source), out var institution))
         {
            ctx.SetPosition(ibn.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.UnknownObjectKey,
                                           actionName,
                                           ibn.KeyNode.GetLexeme(source),
                                           nameof(Institution));
            validation = false;
            continue;
         }

         foreach (var sn in ibn.Children)
         {
            if (!sn.IsContentNode(ctx, source, actionName, ref validation, out var cn))
               continue;

            var key = cn.KeyNode.GetLexeme(source);
            if (string.Equals(key, activeKey, StringComparison.Ordinal))
               cn.SetBoolIfValid(ctx, actionName, source, institution, Institution.Field.IsActive, ref validation);
            else if (string.Equals(key, birthPlaceKey, StringComparison.Ordinal))
               cn.SetLocationIfValid(ctx,
                                     actionName,
                                     source,
                                     institution,
                                     Institution.Field.BirthPlace,
                                     ref validation);
            else
            {
               ctx.SetPosition(cn.KeyNode);
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.InvalidContentKeyOrType,
                                              actionName,
                                              key,
                                              $"{activeKey}' or '{birthPlaceKey}'");
               validation = false;
            }
         }
      }
   }

   private static void ParseReligionManager(BlockNode bn, LocationContext ctx, string source, ref bool validation)
   {
      const string relationKey = "relation";
      const string actionNameLocal = $"{nameof(InstitutionAndReligiousSchoolsParsing)}.{nameof(ParseReligionManager)}";
      foreach (var sn in bn.Children)
      {
         if (!sn.IsBlockNode(ctx, source, nameof(InstitutionAndReligiousSchoolsParsing), ref validation, out var bn2))
            continue;

         var key = bn2.KeyNode.GetLexeme(source);
         if (!Globals.ReligiousSchools.TryGetValue(key, out var school))
         {
            ctx.SetPosition(bn2.KeyNode);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.UnknownObjectKey,
                                           actionNameLocal,
                                           key,
                                           nameof(ReligiousSchool));
            validation = false;
            continue;
         }

         foreach (var bnSn in bn2.Children)
            if (bnSn is BlockNode ibn &&
                string.Equals(ibn.KeyNode.GetLexeme(source), relationKey, StringComparison.Ordinal))
               foreach (var relationSn in ibn.Children)
                  ValidateAndAddSchoolRelations(ctx, source, ref validation, relationSn, actionNameLocal, school);
            else
               ValidateAndAddSchoolRelations(ctx, source, ref validation, bnSn, actionNameLocal, school);
      }
   }

   private static void ValidateAndAddSchoolRelations(LocationContext ctx,
                                                     string source,
                                                     ref bool validation,
                                                     StatementNode relationSn,
                                                     string actionNameLocal,
                                                     ReligiousSchool school)
   {
      if (!relationSn.IsContentNode(ctx, source, actionNameLocal, ref validation, out var rcn))
         return;

      var relKey = rcn.KeyNode.GetLexeme(source);
      if (!Globals.ReligiousSchools.TryGetValue(relKey, out var left))
      {
         ctx.SetPosition(rcn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.UnknownObjectKey,
                                        actionNameLocal,
                                        relKey,
                                        nameof(ReligiousSchool));
         validation = false;
         return;
      }

      if (!rcn.Value.IsLiteralValueNode(ctx, actionNameLocal, ref validation, out var lvn))
         return;

      if (!lvn.GetEnum(ctx,
                       actionNameLocal,
                       source,
                       typeof(ReligiousSchoolRelationType),
                       ref validation,
                       out var enumObj))
         return;

      school.Relations.Add(new(left, (ReligiousSchoolRelationType)enumObj));
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
      => throw new NotImplementedException();
}