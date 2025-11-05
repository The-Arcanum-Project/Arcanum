using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Country), IgnoredBlockKeys = ["variables"])]
public partial class CountryParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : ParserValidationLoadingService<Country>(dependencies)
{
   public override bool IsHeavyStep => true;

   private static void ValidateAndParseCountries(BlockNode rootBn,
                                                 LocationContext ctx,
                                                 string source,
                                                 Eu5FileObj fileObj,
                                                 ref bool validation)
   {
      if (!Parser.EnforceNodeCountOfType(rootBn.Children,
                                         1,
                                         ctx,
                                         nameof(ValidateAndParseCountries),
                                         out List<BlockNode> cn2S))
         return;

      Dictionary<string, Country> tagCheck = new();
      SimpleObjectParser.Parse(fileObj,
                               cn2S[0].Children,
                               ctx,
                               nameof(CountryParsing),
                               source,
                               ref validation,
                               ParseProperties,
                               tagCheck,
                               false);

      foreach (var country in tagCheck.Values)
         Globals.Countries[country.UniqueId] = country;
   }

   private static void HandleCurrentAgeParsing(ContentNode rootCn, string source, LocationContext ctx)
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
                                     nameof(CountryParsing),
                                     rootCn.KeyNode.GetLexeme(source),
                                     currentAgeKey);
   }

   protected override void LoadSingleFile(RootNode rn,
                                          LocationContext ctx,
                                          Eu5FileObj fileObj,
                                          string actionStack,
                                          string source,
                                          ref bool validation,
                                          object? lockObject)
   {
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
               const string countriesKey = "countries";
               if (rootBn.KeyNode.GetLexeme(source).Equals(countriesKey))
                  ValidateAndParseCountries(rootBn, ctx, source, fileObj, ref validation);
               else
                  DiagnosticException.LogWarning(ctx.GetInstance(),
                                                 ParsingError.Instance.InvalidBlockNames,
                                                 actionStack,
                                                 rootBn.KeyNode.GetLexeme(source),
                                                 new[] { countriesKey });

               break;
            }
         }
      }
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Country target,
                                                   LocationContext ctx,
                                                   string source,
                                                   ref bool validation,
                                                   bool allowUnknownNodes)
      => ParseProperties(block, target, ctx, source, ref validation, allowUnknownNodes);
}