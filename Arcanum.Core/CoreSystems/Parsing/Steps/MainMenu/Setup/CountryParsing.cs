using System.Diagnostics;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup;

[ParserFor(typeof(Country))]
public partial class CountryParsing
{
   protected internal static void LoadSingleFile(List<StatementNode> sns,
                                                 LocationContext ctx,
                                                 Eu5FileObj<Country> fileObj,
                                                 string actionStack,
                                                 string source,
                                                 ref bool validation)
   {
      Dictionary<string, Country> tagCheck = new();
      SimpleObjectParser.Parse(fileObj,
                               sns,
                               ctx,
                               actionStack,
                               source,
                               ref validation,
                               ParseProperties,
                               tagCheck,
                               false);

      foreach (var country in tagCheck.Values)
         Globals.Countries[country.UniqueId] = country;
   }
}