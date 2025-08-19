using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Economy;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class MarketParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Market)];

   public override string GetFileDataDebugInfo()
   {
      return $"Loaded '{Globals.Locations.Values.Count(x => x.HasMarket)}' markets";
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (blocks, contents) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);
      
      if (blocks.Count != 1)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        GetActionName(),
                                        fileObj.Path.FullPath,
                                        1,
                                        blocks.Count);
         return false;
      }
      
      if (contents.Count != 0)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        0,
                                        contents.Count,
                                        fileObj.Path.FullPath);
         return false;
      }
      
      var block = blocks[0];
      ctx.LineNumber = block.StartLine;

      if (!block.Name.Equals("market_manager"))
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockName,
                                        GetActionName(),
                                        block.Name,
                                        "market_manager");
         return false;
      }
      
      if (block.SubBlocks.Count != 0)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        GetActionName(),
                                        block.Name,
                                        0,
                                        block.SubBlocks.Count);
         return false;
      }
      
      if (block.ContentElements.Count != 1)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentElementCount,
                                        GetActionName(),
                                        1,
                                        block.ContentElements.Count,
                                        block.Name);
         return false;
      }
      
      var flawless = true;
      foreach (var kvp in block.ContentElements[0].GetLineKvpEnumerator(fileObj.Path))
      {
         if (!kvp.Key.Equals("add_market"))
         {
            ctx.ColumnNumber = kvp.Line;
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.UnexpectedKeyInKeyValuePair,
                                           GetActionName(),
                                           kvp.Key,
                                           "add_market");
            flawless = false;
            continue;
         }
         
         if (Globals.Locations.TryGetValue(kvp.Value, out var location))
            location.Market = Market.Exists;
         else
         {
            ctx.ColumnNumber = kvp.Line;
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                         ParsingError.Instance.InvalidLocationKey,
                                         GetActionName(),
                                         kvp.Key);
            flawless = false;
         }
      }
      
      return flawless;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      foreach (var location in Globals.Locations.Values)
         location.Market = Market.Empty;
      return true;
   }
}