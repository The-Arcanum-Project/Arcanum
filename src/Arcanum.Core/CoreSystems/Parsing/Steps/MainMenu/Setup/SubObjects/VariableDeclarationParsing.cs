using Arcanum.Core.CoreSystems.Parsing.NodeParser.ToolBox;
using VariableDataBlock = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects.VariableDataBlock;
using VariableDeclaration = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.SubObjects.VariableDeclaration;

namespace Arcanum.Core.CoreSystems.Parsing.Steps.MainMenu.Setup.SubObjects;

[ParserFor(typeof(VariableDeclaration))]
public static partial class VariableDeclarationParsing;

[ParserFor(typeof(VariableDataBlock))]
public static partial class VariableDataBlockParsing;