using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GameObjects.Map.SubObjects;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

[ParserFor(typeof(LocationTemplateData))]
public partial class LocationTemplateParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<LocationTemplateData>(dependencies)
{
   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      var result = base.UnloadSingleFileContent(fileObj, lockObject);
      foreach (var obj in fileObj.ObjectsInFile)
         if (Globals.Locations.TryGetValue(obj.UniqueId, out var loc))
            loc.TemplateData = LocationTemplateData.Empty;
      return result;
   }

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ref pc,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   public override bool AfterLoadingStep(FileDescriptor descriptor)
   {
      var locations = Globals.Locations;
      foreach (var ltd in Globals.LocationTemplateData.Values)
         if (locations.TryGetValue(ltd.UniqueId, out var loc))
            loc.TemplateData = ltd;
         else
            De.Warning(ltd.FileLocation.ToLocationContext(ltd.Source),
                       ParsingError.Instance.UnknownObjectKey,
                       $"{nameof(LocationTemplateParsing)}.AssignTemplates",
                       ltd.UniqueId,
                       nameof(Location));
      return true;
   }

   private static bool MapMovementAssistParsing(BlockNode node,
                                                LocationTemplateData target,
                                                ref ParsingContext pc)
   {
      target.MovementAssistance = Eu5Activator.CreateEmbeddedInstance<MapMovementAssist>(null, node);
      var index = 0;
      foreach (var sn in node.Children)
      {
         float value;
         if (sn is KeyOnlyNode kon)
         {
            if (!kon.TryParseFloatValue(ref pc, out value))
            {
               index++;
               continue;
            }
         }
         else
         {
            if (!sn.IsUnaryStatementNode(ref pc, out var usn))
               continue;

            if (!usn.TryParseFloatValue(ref pc, out value))
            {
               index++;
               continue;
            }
         }

         switch (index)
         {
            case 0:
               target.MovementAssistance.X = value;
               break;
            case 1:
               target.MovementAssistance.Y = value;
               break;
            default:
               pc.SetContext(node);
               De.Warning(ref pc,
                          ParsingError.Instance.UnexpectedTokenCount,
                          2,
                          index + 1);
               pc.Fail();
               break;
         }

         index++;
      }

      return true;
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   LocationTemplateData target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}