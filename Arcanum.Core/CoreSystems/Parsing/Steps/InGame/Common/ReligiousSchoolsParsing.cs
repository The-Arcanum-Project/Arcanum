using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.ToolBox;
using Arcanum.Core.GameObjects.Religion;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

[ParserFor(typeof(ReligiousSchool))]
public partial class ReligiousSchoolsParsing() : DiscoverThenParseLoadingService<ReligiousSchool>(true);