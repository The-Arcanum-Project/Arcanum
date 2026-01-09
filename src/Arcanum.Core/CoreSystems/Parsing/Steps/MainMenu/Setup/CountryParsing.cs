using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup.SubObjects;
using Arcanum.Core.CoreSystems.Parsing.Steps.Setup;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.InGame.AbstractMechanics;
using Arcanum.Core.GameObjects.InGame.Court.State;
using Arcanum.Core.Utils.Sorting;
using Country = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Country;
using VariableDeclaration = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects.VariableDeclaration;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Country))]
public partial class CountryParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : SetupFileLoadingService(dependencies)
{
   public override bool IsHeavyStep => true;
   public override List<Type> ParsedObjects { get; } =
   [
      .. SetupParsingManager.NestedSubTypes(Country.Empty), .. SetupParsingManager.NestedSubTypes(GovernmentState.Empty), typeof(Age)
   ];

   public override void ReloadSingleFile(Eu5FileObj fileObj, object? lockObject)
   {
   }

   public override bool UnloadSingleFileContent(Eu5FileObj fileObj, object? lockObject)
   {
      return true;
   }

   public override void LoadSetupFile(StatementNode sn, ref ParsingContext pc, Eu5FileObj fileObj, object? lockObject)
   {
      if (!sn.IsBlockNode(ref pc, out var bn))
         return;

      const string countriesKey = "countries";
      if (pc.IsSliceEqual(bn, countriesKey))
         bn = ((BlockNode)sn).Children[0] as BlockNode;

      foreach (var cn in bn!.Children)
      {
         if (!cn.IsBlockNode(ref pc, out var countryBn))
            continue;

         var key = pc.SliceString(countryBn);
         if (!Globals.Countries.TryGetValue(key, out var eu5Obj))
         {
            pc.SetContext(countryBn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.UnknownKey,
                                           key,
                                           countryBn);
            continue;
         }

         ParseProperties(countryBn, eu5Obj, ref pc, false);
      }
   }

   public static bool ArcParse_Variables(BlockNode node, Country country, ref ParsingContext pc)
   {
      if (!node.Children[0].IsBlockNode(ref pc, out var dataContainer))
         return false;

      if (!pc.IsSliceEqual(dataContainer, "data"))
      {
         pc.SetContext(dataContainer);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidBlockName,
                                        "data",
                                        dataContainer);
         return false;
      }

      foreach (var varc in dataContainer.Children)
      {
         if (!varc.IsBlockNode(ref pc, out var varBlock))
            continue;

         if (!pc.IsSliceEqual(varBlock, "{"))
         {
            pc.SetContext(varBlock);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidBlockName,
                                           "Array Declaration Block",
                                           varBlock);
            continue;
         }

         var vard = new VariableDeclaration();
         foreach (var sn in varBlock.Children)
            VariableDeclarationParsing.Dispatch(sn, vard, ref pc);

         country.Variables.Add(vard);
      }

      return true;
   }
}