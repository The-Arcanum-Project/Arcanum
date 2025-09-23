using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Culture.SubObjects;
using Arcanum.Core.GameObjects.MainMenu.States;
using Arcanum.Core.GameObjects.Religion.SubObjects;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class InstitutionStateReligiousSchoolStateParsing : PureParseLoadingService
{
   public override List<Type> ParsedObjects => [typeof(InstitutionManager), typeof(ReligiousSchoolRelations)];
   public override string GetFileDataDebugInfo() => "Parsed InstitutionState and ReligiousSchoolState";

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.State.InstitutionManager = new();
      Globals.State.ReligiousSchoolRelations = new();
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      if (rn.Statements.Count != 2)
      {
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.InvalidNodeCountOfType,
                                        actionStack,
                                        "null",
                                        2,
                                        rn.Statements.Count);
         return;
      }

      InstitutionStateManager.ParseCreateObject(rn.Statements[0], ctx, fileObj, source, actionStack, ref validation);
      // TODO: Talk to @Melco for ideas, but ultimately custom parser?
      // ReligiousSchoolRelationsParsing.ParseCreateObject(rn.Statements[1], ctx, fileObj, source, actionStack, ref validation);
   }
}

[ParserFor(typeof(InstitutionManager))]
public abstract partial class InstitutionStateManager : ParserValidationLoadingService<InstitutionManager>
{
   public static void ParseCreateObject(StatementNode sn,
                                        LocationContext ctx,
                                        Eu5FileObj fileObj,
                                        string source,
                                        string actionStack,
                                        ref bool validation)
   {
      if (!SimpleObjectParser.Parse(fileObj,
                                    sn,
                                    ctx,
                                    actionStack,
                                    source,
                                    ref validation,
                                    ParseProperties,
                                    out InstitutionManager? im))
         return;

      Globals.State.InstitutionManager = im!;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
   }
}

[ParserFor(typeof(InstitutionState))]
public abstract partial class InstitutionStateParsing;

[ParserFor(typeof(ReligiousSchoolRelations))]
public partial class ReligiousSchoolRelationsParsing
   : ParserValidationLoadingService<ReligiousSchoolRelations>
{
   public static void ParseCreateObject(StatementNode sn,
                                        LocationContext ctx,
                                        Eu5FileObj fileObj,
                                        string source,
                                        string actionStack,
                                        ref bool validation)
   {
      SimpleObjectParser.Parse(fileObj,
                               ((BlockNode)sn).Children,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               ReligiousSchoolRelations.GetGlobalItems(),
                               null);
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
   }
}