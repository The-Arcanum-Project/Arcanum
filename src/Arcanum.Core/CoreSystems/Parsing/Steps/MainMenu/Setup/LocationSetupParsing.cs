using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using PopDefinition = Arcanum.Core.GameObjects.InGame.Pops.PopDefinition;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Location))]
public partial class LocationSetupParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override List<Type> ParsedObjects => [typeof(Location), typeof(PopDefinition)];

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
      // We reach this up to the manager to invoke us again with the correct context.
      SetupParsingManager.ReloadFileByService<LocationSetupParsing>(fileObj,
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

      CommentNode? lastComment = null;
      foreach (var cn in bn.Children)
      {
         if (cn is CommentNode comNode)
         {
            lastComment = comNode;
            continue;
         }

         if (!cn.IsBlockNode(ref pc, out var objBn))
            continue;

         var locName = pc.SliceString(objBn);
         if (!Globals.Locations.TryGetValue(locName, out var loc))
         {
            De.Warning(ref pc,
                       ParsingError.Instance.InvalidLocationKey,
                       locName);
            pc.Fail();
            continue;
         }

         if (lastComment is not null)
         {
            loc.AddStandaloneComment(lastComment.CommentText);
            lastComment = null;
         }

         ParseProperties(objBn, loc, ref pc, false);
      }
   }
}