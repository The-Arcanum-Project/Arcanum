using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class LocationRankLoading : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(LocationRank)];

   public override string GetFileDataDebugInfo()
   {
      var outStr = $"Location Ranks: ({Globals.LocationRanks.Count})\n";
      return Globals.LocationRanks.Aggregate(outStr,
                                             (current, rank)
                                                => current + $"- {rank.Name} ({rank.ColorKey}, {rank.IsMaxRank})\n");
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (blocks, contents) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (contents.Count != 0)
      {
         ctx.LineNumber = contents[0].StartLine;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentElementCount,
                                        nameof(LocationFileLoading).GetType().FullName!,
                                        0,
                                        contents.Count,
                                        fileObj.Path);
      }

      var count = 0;
      foreach (var block in blocks)
      {
         if (Globals.LocationRanks.FirstOrDefault(r => r.Name.Equals(block.Name, StringComparison.OrdinalIgnoreCase)) !=
             null)
         {
            ctx.LineNumber = block.StartLine;

            DiagnosticException.CreateAndHandle(ctx.GetInstance(),
                                                ParsingError.Instance.DuplicateObjectDefinition,
                                                GetActionName(),
                                                DiagnosticSeverity.Error,
                                                DiagnosticReportSeverity.PopupWarning,
                                                block.Name,
                                                nameof(LocationRank),
                                                nameof(LocationRank.Name));
            count++;
            continue;
         }

         LocationRank rank = new(block.Name, count);

         var values =
            GetKeyValues.GetKeyValuesFromContents(block.ContentElements, ["color", "?max_rank"], ctx, fileObj.Path);


         if (!string.IsNullOrEmpty(values[1]))
         {
            ValuesParsing.ParseBool(values[1], ctx, GetActionName(), out var maxRank);
            rank.IsMaxRank = maxRank;
         }
         rank.ColorKey = values[0];
         
         Globals.LocationRanks.Add(rank);
         count++;
      }

      return true;
   }

   /// <summary>
   /// There is only one file allowed so we clear the list on unload
   /// </summary>
   /// <param name="fileObj"></param>
   /// <param name="descriptor"></param>
   /// <returns></returns>
   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.LocationRanks.Clear();
      return true;
   }
}