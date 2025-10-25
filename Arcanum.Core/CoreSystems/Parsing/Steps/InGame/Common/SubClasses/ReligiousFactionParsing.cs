using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.Utils.Sorting;
using ReligiousFaction = Arcanum.Core.GameObjects.Religious.ReligiousFaction;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common.SubClasses;

public class ReligiousFactionParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ReligiousFaction>(true, dependencies);