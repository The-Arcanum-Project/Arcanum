using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(PopDefinition))]
public partial class PopsParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<PopDefinition>(dependencies)
{
   public override bool IsHeavyStep => true;

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
      if (rn.Statements.Count != 1)
      {
         De.Warning(ctx,
                    ParsingError.Instance.InvalidNodeCountOfType,
                    actionStack,
                    "undefined",
                    rn.Statements.Count,
                    1);
         validation = false;
         return;
      }

      if (!rn.Statements[0].IsBlockNode(ctx, source, actionStack, ref validation, out var locationNode))
         return;

      foreach (var ln in locationNode.Children)
      {
         if (!ln.IsBlockNode(ctx, source, actionStack, ref validation, out var popNode))
            continue;

         var locName = popNode.KeyNode.GetLexeme(source);
         if (!Globals.Locations.TryGetValue(locName, out var loc))
         {
            De.Warning(ctx,
                       ParsingError.Instance.InvalidLocationKey,
                       actionStack,
                       locName);
            validation = false;
            continue;
         }

         foreach (var pn in popNode.Children)
         {
            if (!pn.IsBlockNode(ctx, source, actionStack, ref validation, out var popDefNode))
               continue;

            var definition = new PopDefinition();
            ParseProperties(popDefNode, definition, ctx, source, ref validation, false);
            loc.Pops.Add(definition);
         }
      }
   }
}