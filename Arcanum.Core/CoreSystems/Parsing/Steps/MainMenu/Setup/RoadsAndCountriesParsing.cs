using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Road = Arcanum.Core.GameObjects.Map.Road;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

public class RoadsAndCountriesParsing : FileLoadingService
{
   public override List<Type> ParsedObjects { get; } = [typeof(Road), typeof(Country)];

   public override string GetFileDataDebugInfo()
   {
      return $"\nCountries: {Globals.Countries.Count} entries\n" +
             $"Roads: {Globals.Roads.Count} entries";
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      // TODO: @MelCo only remove the ones from the file being unloaded
      // This can only be done once we have the working Saveable system
      Globals.Countries.Clear();
      Globals.Countries.TrimExcess();
      Globals.Roads.Clear();
      Globals.Roads.TrimExcess();
      return true;
   }

   public override bool LoadSingleFile(Eu5FileObj fileObj, FileDescriptor descriptor, object? lockObject)
   {
      var currentAge = string.Empty;
      var rn = Parser.Parse(fileObj, out var source);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);
      var validation = true;

      Parser.VerifyNodeTypes([..rn.Statements],
                             [typeof(BlockNode), typeof(ContentNode)],
                             ctx,
                             GetActionName());
      foreach (var rootStatement in rn.Statements)
      {
         switch (rootStatement)
         {
            case ContentNode rootCn:
            {
               HandleCurrentAgeParsing(rootCn, source, ctx);
               continue;
            }

            case BlockNode rootBn:
            {
               const string roadNetworkKey = "road_network";
               const string countriesKey = "countries";
               if (rootBn.KeyNode.GetLexeme(source).Equals(roadNetworkKey))
                  ProcessRoadNode(rootBn, ctx, source, new(fileObj.Path, fileObj.Descriptor));
               else if (rootBn.KeyNode.GetLexeme(source).Equals(countriesKey))
                  ValidateAndParseCountries(rootBn, ctx, source, fileObj, ref validation);
               else
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.InvalidBlockNames,
                                                 GetActionName(),
                                                 rootBn.KeyNode.GetLexeme(source),
                                                 new[] { roadNetworkKey, countriesKey });

               break;
            }
         }
      }

      return true;
   }

   private void ValidateAndParseCountries(BlockNode rootBn,
                                          LocationContext ctx,
                                          string source,
                                          Eu5FileObj fileObj,
                                          ref bool validation)
   {
      if (!Parser.EnforceNodeCountOfType(rootBn.Children,
                                         1,
                                         ctx,
                                         GetActionName(),
                                         out List<BlockNode> cn2S))
         return;

      Eu5FileObj fo = new(fileObj.Path, fileObj.Descriptor);
      CountryParsing.LoadSingleFile(cn2S[0].Children, ctx, fo, GetActionName(), source, ref validation);
   }

   private void ProcessRoadNode(BlockNode rootBn, LocationContext ctx, string source, Eu5FileObj fileObj)
   {
      foreach (var rdNode in rootBn.Children)
      {
         var create = true;
         if (!Parser.GetIdentifierKvp(rdNode, ctx, GetActionName(), source, out var key, out var value))
            continue;

         if (!ValuesParsing.ParseLocation(key, ctx, GetActionName(), out var start))
            create = false;
         if (!ValuesParsing.ParseLocation(value, ctx, GetActionName(), out var end))
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
            Globals.Roads.TryAdd($"{start}-{end}",
                                 new()
                                 {
                                    StartLocation = start,
                                    EndLocation = end,
                                    Source = fileObj,
                                 });
      }
   }

   private void HandleCurrentAgeParsing(ContentNode rootCn, string source, LocationContext ctx)
   {
      string currentAge;
      const string currentAgeKey = "current_age";
      if (rootCn.KeyNode.GetLexeme(source).Equals(currentAgeKey) && rootCn.Value is LiteralValueNode lvn)
      {
         currentAge = lvn.Value.GetLexeme(source);
         return;
      }

      ctx.LineNumber = rootCn.KeyNode.Line;
      ctx.ColumnNumber = rootCn.KeyNode.Column;
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidContentKeyOrType,
                                     GetActionName(),
                                     rootCn.KeyNode.GetLexeme(source),
                                     currentAgeKey);
   }
}