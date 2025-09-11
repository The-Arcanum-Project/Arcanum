using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class PopTypeParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(PopType)];

   public override string GetFileDataDebugInfo()
   {
      return $"PopTypes: {Globals.PopTypes.Count} entries";
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor)
   {
      Globals.PopTypes.Clear();
      Globals.PopTypes.TrimExcess();
      return true;
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
                                        nameof(PopTypeParsing).GetType().FullName!,
                                        0,
                                        contents.Count,
                                        fileObj.Path);
      }

      foreach (var block in blocks)
      {
         var values = GetKeyValues.GetKeyValuesFromContents(block.ContentElements,
                                                            [
                                                               "pop_food_consumption", "assimilation_conversion_factor",
                                                               "color",
                                                            ],
                                                            ctx,
                                                            fileObj.Path);

         NumberParsing.TryParseFloat(values[0], ctx, out var foodConsumption, 0f, fallback: 0f);
         NumberParsing.TryParseFloat(values[1], ctx, out var assimilationConversionFactor, 0f, fallback: 0f);

         Globals.PopTypes.Add(block.Name,
                              new(block.Name,
                                  values[2],
                                  foodConsumption,
                                  assimilationConversionFactor));
      }

      return true;
   }

   public override bool IsFullyParsed { get; } = false;
}