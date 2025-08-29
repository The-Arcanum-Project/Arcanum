using System.Collections.Immutable;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
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
      var currentAge = string.Empty;
      var rn = Parser.Parse(fileObj, out var source);
      var ctx = new LocationContext(0, 0, fileObj.Path.FullPath);

      Parser.VerifyNodeTypes([..rn.Statements.Cast<AstNode>()],
                             [typeof(BlockNode), typeof(ContentNode)],
                             ctx,
                             GetActionName());
      foreach (var rootStatement in rn.Statements)
      {
         switch (rootStatement)
         {
            case ContentNode rootCn:
            {
               const string currentAgeKey = "current_age";
               if (rootCn.KeyNode.GetLexeme(source).Equals(currentAgeKey) && rootCn.Value is LiteralValueNode lvn)
               {
                  currentAge = lvn.Value.GetLexeme(source);
                  continue;
               }

               ctx.LineNumber = rootCn.KeyNode.Line;
               ctx.ColumnNumber = rootCn.KeyNode.Column;
               DiagnosticException.LogWarning(ctx.GetInstance(),
                                              ParsingError.Instance.InvalidContentKeyOrType,
                                              GetActionName(),
                                              rootCn.KeyNode.GetLexeme(source),
                                              ctx.LineNumber,
                                              ctx.ColumnNumber,
                                              currentAgeKey);
               continue;
            }

            case BlockNode rootBn:
            {
               const string roadNetworkKey = "road_network";
               const string countriesKey = "countries";
               if (rootBn.KeyNode.GetLexeme(source).Equals(roadNetworkKey))
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
                        Globals.Roads.Add(new(start, end));
                  }
               else if (rootBn.KeyNode.GetLexeme(source).Equals(countriesKey))
               {
                  if (!Parser.EnforceNodeCountOfType(rootBn.Children,
                                                     1,
                                                     ctx,
                                                     GetActionName(),
                                                     out List<BlockNode> cn2S))
                     continue;

                  // Layered countries block
                  var cn2 = cn2S[0];
                  Parser.EnforceNodeCountOfType(cn2.Children, -1, ctx, GetActionName(), out List<BlockNode> cNodes);
                  foreach (var cNode in cNodes)
                  {
                     ParseCountryFromNode(cNode, ctx, source);
                  }
               }
               else
               {
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.InvalidBlockNames,
                                                 GetActionName(),
                                                 rootBn.KeyNode.GetLexeme(source),
                                                 new[] { roadNetworkKey, countriesKey });
               }

               break;
            }
         }
      }

      return true;
   }

   private static readonly ImmutableArray<string> collectionKeys =
   [
      "own_control_core", "own_control_integrated", "own_control_conquered", "own_control_colony", "own_core",
      "own_conquered", "own_integrated", "own_colony", "control_core", "control", "our_cores_conquered_by_others",
   ];

   private static void ParseCountryFromNode(BlockNode cNode, LocationContext ctx, string source)
   {
      Tag tag = new(cNode.KeyNode.GetLexeme(source));

      if (!tag.IsValid)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidTagFormat,
                                        "Parsing Country",
                                        cNode.KeyNode.GetLexeme(source));
         return;
      }

      Country country = new(tag);

      foreach (var sNode in cNode.Children)
      {
         if (sNode is BlockNode bNode)
         {
            var key = bNode.KeyNode.GetLexeme(source);
            if (!collectionKeys.Contains(key))
            {
               // We have either a government, accepted_cultures, tolerated_cultures
            }
            else
            {
               // We have any kind of the location collections
               country.SetCollection(key,
                                     LUtil.LocationsFromStatementNodes(bNode.Children,
                                                                       ctx,
                                                                       "Country collection parsing",
                                                                       source));
            }
         }
         else if (sNode is ContentNode cn)
         {
            var cnKey = cn.KeyNode.GetLexeme(source);
            switch (cnKey)
            {
               case "include":
                  
                  break;
               
               case "capital":
                  break;
               
               case "dynasty":
                  break;
               
               case "country_rank":
                  break;
               
               case "starting_technology_level":
                  break;
               
               default:
                  Console.WriteLine($"Unknown country property: {cnKey}");
                  ctx.SetPosition(cn.KeyNode);
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.InvalidContentKeyOrType,
                                                 "Parsing Country",
                                                 cnKey,
                                                 ctx.LineNumber,
                                                 ctx.ColumnNumber,
                                                 "capital, dynasty, country_rank, starting_technology_level, include");
                  break;
            }
         }
         else
         {
            ctx.LineNumber = sNode.KeyNode.Line;
            ctx.ColumnNumber = sNode.KeyNode.Column;
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidBlockType,
                                           "Parsing Country",
                                           ctx.LineNumber,
                                           ctx.ColumnNumber,
                                           sNode.GetType().Name,
                                           "BlockNode or ContentNode");
         }
      }

      Globals.Countries[tag] = country;
   }
}