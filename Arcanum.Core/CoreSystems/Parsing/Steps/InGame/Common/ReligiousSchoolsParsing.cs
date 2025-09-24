using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Religion;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(ReligiousSchool))]
public partial class ReligiousSchoolsParsing() : DiscoverThenParseLoadingService<ReligiousSchool>(true);