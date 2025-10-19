using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Religion.SubObjects;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class ReligiousFocusParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<ReligiousFocus>(true, dependencies);