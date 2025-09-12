using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(Topography))]
public partial class TopographyParsing : ParserValidationLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Topography)];
   public override string GetFileDataDebugInfo() => $"Parsed Topographies: {Globals.Topography.Count}";

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.Topography.Clear();
      return true;
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          string actionStack,
                                          string source,
                                          ref bool validation)
   {
      SimpleObjectParser.Parse(rn, ctx, actionStack, source, ref validation, ParseProperties, Globals.Topography);
   }
}