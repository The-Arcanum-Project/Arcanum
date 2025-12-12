using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using Arcanum.Core.GameObjects.LocationCollections.SubObjects;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup.SubObjects;

[ParserFor(typeof(VariableDeclaration))]
public static partial class VariableDeclarationParsing;

[ParserFor(typeof(VariableDataBlock))]
public static partial class VariableDataBlockParsing;