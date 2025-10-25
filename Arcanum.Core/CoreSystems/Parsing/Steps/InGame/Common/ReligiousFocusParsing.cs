using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.Utils.Sorting;
using ReligiousFocus = Arcanum.Core.GameObjects.Religious.SubObjects.ReligiousFocus;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class ReligiousFocusParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ReligiousFocus>(true, dependencies);