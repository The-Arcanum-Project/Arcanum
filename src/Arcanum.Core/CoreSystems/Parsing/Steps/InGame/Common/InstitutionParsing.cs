using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.SavingSystem.Util;
using Arcanum.Core.Utils.Sorting;
using Institution = Arcanum.Core.GameObjects.Cultural.Institution;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class InstitutionParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Institution>(true, dependencies)
{
   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Institution target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes)
      => throw new NotSupportedException("InstitutionParsing should only be used in discovery phase.");
}

[ParserFor(typeof(Institution))]
public partial class InstitutionPropertyParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<Institution>(false, dependencies)
{
   protected override void LoadSingleFileProperties(RootNode rn,
                                                    ref ParsingContext pc,
                                                    Eu5FileObj fileObj,
                                                    object? lockObject)
   {
      SimpleObjectParser.Parse(fileObj,
                               rn,
                               ref pc,
                               ParseProperties,
                               GetGlobals(),
                               lockObject);
   }

   protected override void ParsePropertiesToObject(BlockNode block,
                                                   Institution target,
                                                   ref ParsingContext pc,
                                                   bool allowUnknownNodes) => ParseProperties(block, target, ref pc, allowUnknownNodes);
}