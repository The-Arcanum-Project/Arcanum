using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.GameObjects.Court.State.SubClasses;
using Arcanum.Core.Utils.Sorting;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

public class DesignateHeirReasonParsing(IEnumerable<IDependencyNode<string>> dependencies)
   : DiscoverThenParseLoadingService<DesignateHeirReason>(true, dependencies);