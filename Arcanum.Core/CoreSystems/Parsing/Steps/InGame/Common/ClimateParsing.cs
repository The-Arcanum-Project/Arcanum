using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Climate))]
public partial class ClimateParsing : ParserValidationLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Climate)];
   public override string GetFileDataDebugInfo() => $"Parsed Climates: {Globals.Climates.Count}";

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.Climates.Clear();
      return true;
   }

   public override bool IsFullyParsed => false;

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      // TODO: @Minnator SimpleObjectParser.Parse(rn, ctx, actionStack, source, ref validation, ParseProperties, Globals.Climates);
   }
}