using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class PopsParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Pop)];

   public override string GetFileDataDebugInfo()
   {
      Dictionary<PopType, int> popTypes = new(Globals.PopTypes.Count);
      foreach (var pop in Globals.Locations)
         foreach (var popEntry in pop.Value.Pops)
            if (popTypes.TryGetValue(popEntry.Type, out var count))
               popTypes[popEntry.Type] = count + 1;
            else
               popTypes[popEntry.Type] = 1;

      return $"Pops: {popTypes.Sum(x => x.Value)} entries:\n" +
             string.Join("\n", popTypes.Select(x => $"\t{x.Key.Name,-15} ({x.Key.ColorKey,-15}): {x.Value,-5}"));
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var sw = Stopwatch.StartNew();
      var (blocks, contents) = ElementParser.GetElements(fileObj.Path);
      sw.Stop();
      Debug.WriteLine($"PopsParsing: ElementParser took {sw.ElapsedMilliseconds}ms for {fileObj.Path.FullPath}");
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (contents.Count != 0)
      {
         ctx.LineNumber = contents[0].StartLine;
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentElementCount,
                                        nameof(PopsParsing).GetType().FullName!,
                                        0,
                                        contents.Count,
                                        fileObj.Path);
      }

      if (blocks.Count != 1)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        nameof(PopsParsing).GetType().FullName!,
                                        fileObj.Path,
                                        1,
                                        blocks.Count);
         return false;
      }

      foreach (var block in blocks[0].SubBlocks)
      {
         ctx.LineNumber = block.StartLine;
         if (!LocationChecks.IsValidLocation(ctx, block.Name, out var location))
            continue;

         if (block.SubBlocks.Count < 1 || block.SubBlocks[0].ContentElements.Count < 1)
            continue;

         var popType = PopType.Empty;
         var size = 0f;
         var culture = string.Empty;
         var religion = string.Empty;
         var valid = true;

         foreach (var kvp in block.SubBlocks[0].ContentElements[0].GetLineKvpEnumerator(fileObj.Path))
         {
            switch (kvp.Key)
            {
               case "type":
                  if (!PopType.Empty.Parse(kvp.Value, out popType))
                     valid = false;
                  break;
               case "size":
                  if (!NumberParsing.TryParseFloat(kvp.Value, ctx, out size, 0f, fallback: 0f))
                     valid = false;
                  break;
               case "culture":
                  culture = kvp.Value;
                  break;
               case "religion":
                  religion = kvp.Value;
                  break;
               default:
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.UnknownKey,
                                                 nameof(PopsParsing).GetType().FullName!,
                                                 kvp.Key,
                                                 kvp.Value);
                  valid = false;
                  break;
            }

            if (valid)
               location!.Pops.Add(new(popType!, size, culture, religion));
         }
      }

      return true;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      foreach (var location in Globals.Locations)
         location.Value.Pops.Clear();
      return true;
   }
}