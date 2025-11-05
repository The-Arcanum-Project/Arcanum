using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Common;

[ParserFor(typeof(ModifierDefinition))]
public partial class ModifierParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<ModifierDefinition>(dependencies)
{
   protected override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.ObjectsInFile)
         Globals.ModifierDefinitions.Remove(obj.UniqueId);

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
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               Globals.ModifierDefinitions,
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   ModifierDefinition target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => ParseProperties(block, target, ctx, source, ref validation, allowUnknownNodes);
}