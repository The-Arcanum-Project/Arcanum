using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Road = Arcanum.Core.GameObjects.Map.Road;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class RoadsParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : FileLoadingService(dependencies)
{
   public override bool CanBeReloaded => false;

   public override void ReloadSingleFile(Eu5FileObj fileObj,
                                         object? lockObject)
   {
   }

   public override List<Type> ParsedObjects { get; } = [typeof(Road)];

   public override string GetFileDataDebugInfo()
   {
      return $"\nCountries: {Globals.Countries.Count} entries\n" +
             $"Roads: {Globals.Roads.Count} entries";
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      var globals = Globals.Roads;
      foreach (var road in fileObj.ObjectsInFile)
         globals.Remove(((Road)road).UniqueId);
      return true;
   }

   public override bool LoadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
      var rn = Parser.Parse(fileObj, out var source);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);
      var validation = true;
      var pc = new ParsingContext(ctx, source.AsSpan(), nameof(RoadsParsing), ref validation);

      Parser.VerifyNodeTypes([..rn.Statements],
                             [typeof(BlockNode), typeof(ContentNode)],
                             ref pc);
      foreach (var rootStatement in rn.Statements)
      {
         if (rootStatement is not BlockNode rootBn)
            continue;

         const string roadNetworkKey = "road_network";
         const string countriesKey = "countries";
         if (pc.IsSliceEqual(rootBn.KeyNode, roadNetworkKey))
            ProcessRoadNode(rootBn, ref pc, new(fileObj.Path, fileObj.Descriptor));
         else
         {
            pc.SetContext(rootBn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidBlockNames,
                                           pc.SliceString(rootBn),
                                           new[] { roadNetworkKey, countriesKey });
         }
      }

      return true;
   }

   private void ProcessRoadNode(BlockNode rootBn, ref ParsingContext pc, Eu5FileObj fileObj)
   {
      using var scope = pc.PushScope();
      foreach (var rdNode in rootBn.Children)
      {
         var create = true;
         if (!Parser.GetIdentifierKvp(rdNode, ref pc, out var key, out var value))
            continue;

         if (!ValuesParsing.ParseLocation(key, ref pc, out var start))
            create = false;
         if (!ValuesParsing.ParseLocation(value, ref pc, out var end))
            create = false;

         if (start == end)
         {
            pc.SetContext(rdNode);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidRoadSameLocation,
                                           start.UniqueId);
            create = false;
         }

         if (create)
         {
            Road road = new()
            {
               StartLocation = start,
               EndLocation = end,
               Source = fileObj,
            };
            FileStateManager.RegisterPath(fileObj.Path);
            Globals.Roads.TryAdd($"{start}-{end}", road);
         }
      }
   }
}