using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;

[ParserFor(typeof(ModifierDefinition))]
public partial class ModifierParsing : ParserValidationLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(ModifierDefinition)];
   public override string GetFileDataDebugInfo() => $"Parsed Modifier Definitions: {Globals.ModifierDefinitions.Count}";

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.ModifierDefinitions.Clear();
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      SimpleObjectParser.Parse(rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.ModifierDefinitions);
   }
}