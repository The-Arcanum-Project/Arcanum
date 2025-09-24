using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Religion;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(ReligiousSchool))]
public partial class ReligiousSchoolsParsing(IEnumerable<IDependencyNode<string>> dependencies) : DiscoverThenParseLoadingService<ReligiousSchool>(true, dependencies);