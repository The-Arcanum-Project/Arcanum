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
                                                 ref ParsingContext pc,
                                                 Eu5FileObj fileObj)
   {
      if (!Parser.EnforceNodeCountOfType(rootBn.Children,
                                         1,
                                         ref pc,
                                         out List<BlockNode> cn2S))
         return;

      Dictionary<string, Country> tagCheck = new();
      SimpleObjectParser.Parse(fileObj,
                               cn2S[0].Children,
                               ref pc,
                               ParseProperties,
                               tagCheck,
                               false);

      foreach (var country in tagCheck.Values)
         Globals.Countries[country.UniqueId] = country;
   }

   private static void HandleCurrentAgeParsing(ContentNode rootCn, ref ParsingContext pc)
   {
      // TODO: Use the currentAge variable in custom saving
      // ReSharper disable once NotAccessedVariable
      string currentAge;
      const string currentAgeKey = "current_age";
      if (pc.IsSliceEqual(rootCn, currentAgeKey) && rootCn.Value is LiteralValueNode lvn)
      {
         // ReSharper disable once RedundantAssignment
         currentAge = pc.SliceString(lvn);
         return;
      }

      pc.SetContext(rootCn);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidContentKeyOrType,
                                     pc.SliceString(rootCn),
                                     currentAgeKey);
   }

   public override void LoadSingleFile(RootNode rn,
                                       ref ParsingContext pc,
                                       Eu5FileObj fileObj,
                                       object? lockObject)
   {
      foreach (var rootStatement in rn.Statements)
         switch (rootStatement)
         {
            case ContentNode rootCn:
            {
               HandleCurrentAgeParsing(rootCn, ref pc);
               continue;
            }

            case BlockNode rootBn:
            {
               const string countriesKey = "countries";
               if (pc.IsSliceEqual(rootBn, countriesKey))
                  ValidateAndParseCountries(rootBn, ref pc, fileObj);
               else
                  DiagnosticException.LogWarning(ref pc,
                                                 ParsingError.Instance.InvalidBlockNames,
                                                 pc.SliceString(rootBn),
                                                 new[] { countriesKey });

               break;
            }
         }
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Country target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}