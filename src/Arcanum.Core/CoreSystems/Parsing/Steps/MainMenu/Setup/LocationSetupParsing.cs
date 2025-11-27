using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Location))]
public partial class LocationSetupParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects => [typeof(Location)];

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject,
                                         string actionStack,
                                         ref bool validation)
   {
      // We reach this up to the manager to invoke us again with the correct context.
      SetupParsingManager.ReloadFileByService<LocationSetupParsing>(fileObj,
                                                                    lockObject,
                                                                    actionStack,
                                                                    ref validation);
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      foreach (var obj in fileObj.ObjectsInFile)
      {
         if (obj is not PopDefinition popDef)
            continue;

         if (Globals.Locations.TryGetValue(popDef.UniqueId, out var loc))
            loc.Pops.Remove(popDef);
         else
            // Corrupted application state, location should exist.
            return false;
      }

      return true;
   }

   public override void LoadSetupFile(StatementNode sn,
                                      LocationContext ctx,
                                      Eu5FileObj fileObj,
                                      string actionStack,
                                      string source,
                                      ref bool validation,
                                      object? lockObject)
   {
      if (!sn.IsBlockNode(ctx, source, actionStack, ref validation, out var bn))
         return;

      foreach (var cn in bn.Children)
      {
         if (!cn.IsBlockNode(ctx, source, actionStack, ref validation, out var objBn))
            continue;

         var locName = objBn.KeyNode.GetLexeme(source);
         if (!Globals.Locations.TryGetValue(locName, out var loc))
         {
            De.Warning(ctx,
                       ParsingError.Instance.InvalidLocationKey,
                       actionStack,
                       locName);
            validation = false;
            return;
         }

         ParseProperties(objBn, loc, ctx, source, ref validation, false);
      }
   }
}