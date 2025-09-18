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
public partial class ModifierParsing : ParserValidationLoadingService<ModifierDefinition>
{
   public override List<Type> ParsedObjects { get; } = [typeof(ModifierDefinition)];
   public override string GetFileDataDebugInfo() => $"Parsed Modifier Definitions: {Globals.ModifierDefinitions.Count}";

   protected override bool UnloadSingleFileContent(Eu5FileObj<ModifierDefinition> fileObj)
   {
      foreach (var obj in fileObj.GetEu5Objects())
         Globals.ModifierDefinitions.Remove(obj.UniqueKey);

      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj<ModifierDefinition> fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.ModifierDefinitions);
   }
}