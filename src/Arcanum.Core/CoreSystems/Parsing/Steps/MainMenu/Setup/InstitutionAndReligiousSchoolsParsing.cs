using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.MainMenu.States;
using Arcanum.Core.Utils.Sorting;
using InstitutionState = Arcanum.Core.GameObjects.Cultural.SubObjects.InstitutionState;
using ReligiousSchoolRelations = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousSchoolRelations;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class InstitutionStateReligiousSchoolStateParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : PureParseLoadingService(dependencies)
{
   public override bool CanBeReloaded => false;

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
   }

   public override List<Type> ParsedObjects => [typeof(InstitutionManager), typeof(ReligiousSchoolRelations)];
   public override string GetFileDataDebugInfo() => "Parsed InstitutionState and ReligiousSchoolState";

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.State.InstitutionManager = new();
      Globals.State.ReligiousSchoolRelations = new();
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          ref ParsingContext pc,
                                          Eu5FileObj fileObj,
                                          object? lockObject)
   {
      if (rn.Statements.Count != 2)
      {
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidNodeCountOfType,
                                        "null",
                                        2,
                                        rn.Statements.Count);
         return;
      }

      InstitutionStateManager.ParseCreateObject(rn.Statements[0], ref pc, fileObj);
      // TODO: Talk to @Melco for ideas, but ultimately custom parser?
      // ReligiousSchoolRelationsParsing.ParseCreateObject(rn.Statements[1], ctx, fileObj, source, actionStack, ref validation);
   }
}

[ParserFor(typeof(InstitutionManager))]
public abstract partial class InstitutionStateManager(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<InstitutionManager>(dependencies)
{
   public static void ParseCreateObject(StatementNode sn,
                                        ref ParsingContext pc,
                                        Eu5FileObj fileObj)
   {
      if (!SimpleObjectParser.Parse(fileObj,
                                    sn,
                                    ref pc,
                                    ParseProperties,
                                    out InstitutionManager? im))
         return;

      Globals.State.InstitutionManager = im!;
   }

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
   }
}

[ParserFor(typeof(InstitutionState))]
public abstract partial class InstitutionStateParsing;

[ParserFor(typeof(ReligiousSchoolRelations))]
public partial class ReligiousSchoolRelationsParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<ReligiousSchoolRelations>(dependencies)
{
   public static void ParseCreateObject(StatementNode sn,
                                        ref ParsingContext pc,
                                        Eu5FileObj fileObj)
   {
      SimpleObjectParser.Parse(fileObj,
                               ((BlockNode)sn).Children,
                               ref pc,
                               ParseProperties,
                               ReligiousSchoolRelations.GetGlobalItems(),
                               null);
   }

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
   }

   public override bool CanBeReloaded => false;

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ReligiousSchoolRelations target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}