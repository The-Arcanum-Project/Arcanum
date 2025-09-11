using System.Collections.Immutable;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser;
using Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.KeyWordClasses;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects;
using Arcanum.Core.GameObjects.CountryLevel;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps;

public class RoadsAndCountriesParsing : FileLoadingService
{
   private delegate void CountryAttributeParser(LocationContext ctx, string source, ContentNode cn, Country country);

   private static readonly Dictionary<string, CountryAttributeParser> AttributeParsers = new()
   {
      { CountryKeywords.INCLUDE, ParseIncludes },
      { CountryKeywords.CAPITAL, ParseCapital },
      { CountryKeywords.DYNASTY, DynastyParser },
      { CountryKeywords.COUNTRY_RANK, CountryRankParser },
      { CountryKeywords.STARTING_TECHNOLOGY_LEVEL, StartingTechParser },
      { CountryKeywords.COURT_LANGUAGE, CourtLanguageParser },
      { CountryKeywords.LITURGICAL_LANGUAGE, LiturgicalLanguageParser },
      { CountryKeywords.TYPE, CountryTypeParser },
      { CountryKeywords.RELIGIOUS_SCHOOL, ReligiousSchoolParser },
      { CountryKeywords.REVOLT, RevoltParser },
      { CountryKeywords.IS_VALID_FOR_RELEASE, IsValidForReleaseParser },
      { CountryKeywords.FLAG, DefaultParser },
      { CountryKeywords.COUNTRY_NAME, DefaultParser },
      { CountryKeywords.COLOR, ColorParser },
   };

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
               HandleCurrentAgeParsing(rootCn, source, ctx);
               continue;
            }

            case BlockNode rootBn:
            {
               const string roadNetworkKey = "road_network";
               const string countriesKey = "countries";
               if (rootBn.KeyNode.GetLexeme(source).Equals(roadNetworkKey))
                  ProcessRoadNode(rootBn, ctx, source);
               else if (rootBn.KeyNode.GetLexeme(source).Equals(countriesKey))
                  ValidateAndParseCountries(rootBn, ctx, source);
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

   private void ValidateAndParseCountries(BlockNode rootBn, LocationContext ctx, string source)
   {
      if (!Parser.EnforceNodeCountOfType(rootBn.Children,
                                         1,
                                         ctx,
                                         GetActionName(),
                                         out List<BlockNode> cn2S))
         return;

      // Layered countries block
      var cn2 = cn2S[0];
      Parser.EnforceNodeCountOfType(cn2.Children, -1, ctx, GetActionName(), out List<BlockNode> cNodes);
      foreach (var cNode in cNodes)
      {
         ParseCountryFromNode(cNode, ctx, source);
      }
   }

   private void ProcessRoadNode(BlockNode rootBn, LocationContext ctx, string source)
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
            Globals.Roads.Add(new(start, end));
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

   private static readonly ImmutableArray<string> CollectionKeys =
   [
      "own_control_core", "own_control_integrated", "own_control_conquered", "own_control_colony", "own_core",
      "own_conquered", "own_integrated", "own_colony", "control_core", "control", "our_cores_conquered_by_others",
   ];

   private static void ParseCountryFromNode(BlockNode cNode, LocationContext ctx, string source)
   {
      Tag tag = new(cNode.KeyNode.GetLexeme(source));

      if (!tag.Verify(ctx))
         return;

      Country country = new(tag);

      foreach (var sNode in cNode.Children)
      {
         if (sNode is BlockNode bNode)
         {
            ParseCollectionNodes(ctx, source, bNode, country);
         }
         else if (sNode is ContentNode cn)
         {
            var cnKey = cn.KeyNode.GetLexeme(source);
            ParseCountryAttributes(ctx, source, cnKey, cn, country);
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

   private static void ParseCountryAttributes(LocationContext ctx,
                                              string source,
                                              string cnKey,
                                              ContentNode cn,
                                              Country country)
   {
      if (AttributeParsers.TryGetValue(cnKey, out var parser))
      {
         parser(ctx, source, cn, country);
      }
      else
      {
         // If not found, it's an unknown keyword. 
#if DEBUG
         Console.WriteLine($"Unknown country property: {cnKey}");
#endif
         ctx.SetPosition(cn.KeyNode);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidContentKeyOrType,
                                        "Parsing Country attributes",
                                        cnKey,
                                        string.Join(", ", AttributeParsers.Keys));
      }
   }

   private static void ParseCollectionNodes(LocationContext ctx, string source, BlockNode bNode, Country country)
   {
      var key = bNode.KeyNode.GetLexeme(source);
      if (!CollectionKeys.Contains(key))
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

   #region Strategy Pattern Parsers

   private static void DefaultParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      // No default parsing action
   }

   private static void ParseIncludes(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      if (cn.TryGetStringContentNode(ctx, nameof(ParseCountryFromNode), source, out var include))
         country.Includes.Add(include);
   }

   private static void ParseCapital(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      if (cn.TryParseLocationFromCn(ctx, nameof(ParseCapital), source, out var location))
         country.Capital = location;
   }

   private static void ColorParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      var validation = true;
      cn.SetColorIfValid(ctx, nameof(ColorParser), source, ref validation, country, Country.Field.Color);
   }

   private static void DynastyParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetIdentifierIfValid(ctx, nameof(DynastyParser), source, country, Country.Field.Dynasty);
   }

   private static void CountryRankParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      if (!cn.TryGetIdentifierNode(ctx, nameof(CountryRankParser), source, out var crlName))
         return;

      var crl = Globals.CountryRanks.FirstOrDefault(cr => cr.Name.Equals(crlName));
      if (crl == null)
      {
         ctx.SetPosition(cn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidCountryRankKey,
                                        nameof(CountryRankParser),
                                        crlName,
                                        Globals.CountryRanks.Select(cr => cr.Name));
         return;
      }

      country.CountryRank = crl;
   }

   private static void StartingTechParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetIntegerIfNotX(ctx, nameof(StartingTechParser), source, country, Country.Field.StartingTechLevel);
   }

   private static void CourtLanguageParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetIdentifierIfValid(ctx, nameof(CourtLanguageParser), source, country, Country.Field.CourtLanguage);
   }

   private static void LiturgicalLanguageParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetIdentifierIfValid(ctx, nameof(LiturgicalLanguageParser), source, country, Country.Field.LiturgicalLanguage);
   }

   private static void CountryTypeParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetEnumIfValid(ctx, nameof(CountryTypeParser), source, country, Country.Field.Type, typeof(CountryType));
   }

   private static void ReligiousSchoolParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      if (cn.TryGetIdentifierNode(ctx, nameof(ReligiousSchoolParser), source, out var rsName))
      {
         if (!Globals.ReligiousSchools.TryGetValue(rsName, out var rs))
         {
            ctx.SetPosition(cn.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.UnknownObjectKey,
                                           nameof(ReligiousSchoolParser),
                                           rsName,
                                           nameof(ReligiousSchool));
            return;
         }

         country.ReligiousSchool = rs;
      }
   }

   private static void RevoltParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetBoolIfValid(ctx, nameof(RevoltParser), source, country, Country.Field.Revolt);
   }

   private static void IsValidForReleaseParser(LocationContext ctx, string source, ContentNode cn, Country country)
   {
      cn.SetBoolIfValid(ctx, nameof(IsValidForReleaseParser), source, country, Country.Field.IsValidForRelease);
   }

   #endregion
}