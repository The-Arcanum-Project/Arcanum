using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Common;

/// <summary>
/// The parser for <see cref="TimedModifier"/> instances.
/// </summary>
[ParserFor(typeof(TimedModifier))]
public partial class TimedModifierParser;