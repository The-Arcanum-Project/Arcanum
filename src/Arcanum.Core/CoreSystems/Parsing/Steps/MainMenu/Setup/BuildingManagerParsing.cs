using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.MainMenu.States;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(BuildingsManager))]
public partial class BuildingManagerParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects => [typeof(BuildingDefinition)];

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
      // We reach this up to the manager to invoke us again with the correct context.
      SetupParsingManager.ReloadFileByService<BuildingManagerParsing>(fileObj,
                                                                      lockObject);
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      // TODO: unload all locations defined in this file.

      return true;
   }

   public override void LoadSetupFile(StatementNode sn,
                                      ref ParsingContext pc,
                                      Eu5FileObj fileObj,
                                      object? lockObject)
   {
      if (!sn.IsBlockNode(ref pc, out var bn))
         return;

      ParseProperties(bn, Globals.BuildingsManager, ref pc, false);
   }
}