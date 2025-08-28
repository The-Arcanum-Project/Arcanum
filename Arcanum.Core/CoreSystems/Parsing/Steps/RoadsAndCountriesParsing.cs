using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class RoadsAndCountriesParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Road), typeof(Country), typeof(Tag)];

   public override string GetFileDataDebugInfo()
   {
      return $"\nCountries: {Globals.Countries.Count} entries\n" +
             $"Roads: {Globals.Roads.Count} entries";
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      // TODO: @MelCo only remove the ones from the file being unloaded
      // This can only be done once we have the working Saveable system
      Globals.Countries.Clear();
      Globals.Countries.TrimExcess();
      Globals.Roads.Clear();
      Globals.Roads.TrimExcess();
      return true;
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (blocks, contents) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      foreach (var block in blocks)
      {
         switch (block.Name)
         {
            case "road_network":
               PraseRoadNetwork(block, fileObj.Path, ctx);
               break;
            case "countries":
               ParseCountries(block, fileObj.Path, ctx);
               break;
            default:
               ctx.LineNumber = block.StartLine;
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.InvalidBlockName,
                                              GetActionName(),
                                              block.Name,
                                              "countries");
               break;
         }
      }

      return true;
   }

   private void ParseCountries(Block block, PathObj fileObjPath, LocationContext ctx)
   {
   }

   private void PraseRoadNetwork(Block block, PathObj fileObjPath, LocationContext ctx)
   {
      if (block.SubBlocks.Count != 0)
      {
         ctx.LineNumber = block.StartLine;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        GetActionName(),
                                        block.Name,
                                        0,
                                        block.SubBlocks.Count);
      }

      foreach (var content in block.ContentElements)
      {
         foreach (var kvp in content.GetLineKvpEnumerator(fileObjPath))
         {
            var create = true;
            ctx.LineNumber = kvp.Line;
            if (!ValuesParsing.ParseLocation(kvp.Key, ctx, GetActionName(), out var start))
               create = false;
            if (!ValuesParsing.ParseLocation(kvp.Value, ctx, GetActionName(), out var end))
               create = false;
            
            if (start == end)
            {
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.InvalidRoadSameLocation,
                                              GetActionName(),
                                              start.Name);
               create = false;
            }
            
            if (create)
               Globals.Roads.Add(new (start, end));
         }
      }
   }
}