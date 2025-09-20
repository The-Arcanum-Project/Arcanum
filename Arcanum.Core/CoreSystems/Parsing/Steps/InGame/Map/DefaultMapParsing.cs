using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.Map;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;

public class DefaultMapParsing : FileLoadingService
{
   public override List<Type> ParsedObjects => [typeof(DefaultMapDefinition)];

   public override string GetFileDataDebugInfo()
   {
      return $"IsValid:{Globals.DefaultMapDefinition.IsValid()}\n" +
             $"\tNumber of SoundTolls:\t\t   {Globals.DefaultMapDefinition.SoundTolls.Count}\n" +
             $"\tNumber of Volcanoes:\t\t   {Globals.DefaultMapDefinition.Volcanoes.Count}\n" +
             $"\tNumber of Earthquakes:\t   {Globals.DefaultMapDefinition.Earthquakes.Count}\n" +
             $"\tNumber of SeaZones:\t\t   {Globals.DefaultMapDefinition.SeaZones.Count}\n" +
             $"\tNumber of Lakes:\t\t   {Globals.DefaultMapDefinition.Lakes.Count}\n" +
             $"\tNumber of NonOwnable:\t\t   {Globals.DefaultMapDefinition.NotOwnable.Count}\n" +
             $"\tNumber of ImpassableMountains:  {Globals.DefaultMapDefinition.ImpassableMountains.Count}";
   }

   public override bool LoadSingleFile(FileObj fileObj, FileDescriptor descriptor, object? lockObject = null)
   {
      var (blocks, _) = ElementParser.GetElements(fileObj.Path);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      if (blocks.Count != 7)
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidBlockCount,
                                        GetActionName(),
                                        fileObj.Path.FullPath,
                                        7,
                                        blocks.Count);

      var dmd = Globals.DefaultMapDefinition;

      foreach (var block in blocks)
      {
         if (block.SubBlocks.Count != 0)
         {
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidBlockCount,
                                           GetActionName(),
                                           block.Name,
                                           0,
                                           block.SubBlocks.Count);
            continue;
         }

         ctx.LineNumber = block.StartLine;
         if (block.Name.Equals("sound_toll"))
         {
            var content = block.ContentElements[0];
            foreach (var kvp in content.GetLineKvpEnumerator(fileObj.Path))
               if (Globals.Locations.TryGetValue(kvp.Key, out var startLocation))
               {
                  if (Globals.Locations.TryGetValue(kvp.Value, out var endLocation))
                     dmd.SoundTolls.Add((startLocation, endLocation));
                  else
                     DiagnosticException.LogWarning(ctx.GetInstance(),
                                                    ParsingError.Instance.InvalidLocationKey,
                                                    GetActionName(),
                                                    kvp.Value);
               }
               else
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.InvalidLocationKey,
                                                 GetActionName(),
                                                 kvp.Key);
         }
         else
            dmd.SetCollection(block.Name, ParsingUtil.ParseLocationList(block.ContentElements[0], ctx), ctx);
      }

      return true;
   }

   public override bool UnloadSingleFileContent(FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      Globals.DefaultMapDefinition.NotOwnable = [];
      Globals.DefaultMapDefinition.SeaZones = [];
      Globals.DefaultMapDefinition.Volcanoes = [];
      Globals.DefaultMapDefinition.Earthquakes = [];
      Globals.DefaultMapDefinition.Lakes = [];
      Globals.DefaultMapDefinition.SoundTolls = [];
      Globals.DefaultMapDefinition.ImpassableMountains = [];
      return true;
   }
}