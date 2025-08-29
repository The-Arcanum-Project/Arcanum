# Parsing System Overview

The parsing system is meant to parse the jomini context into blocks and contents which can be processed further. The parsing system is designed to be modular and extensible, allowing for easy addition of new parsing steps as needed. 

## How to add a new parsing step
1. Create a new class that implements the `FileLoadingService` class.
2. Implement all required methods and properties.
3. Register a new `FileDescriptor` in the `ParsingMaster` and list all dependencies which should be loaded first.
4. Can be debugged when launching in debug mode using `F12` and `F9` to execute it or retrieve any generated data.
### Overview: What the Parser Returns

#### Statement Nodes (Entries in the configuration)

| Syntax Example                          | Returns AST Node        | Description                                                                      |
|:----------------------------------------|:------------------------|:---------------------------------------------------------------------------------|
| `graphics = { ... }` or `audio { ... }` | `BlockNode`             | A named container for other statements.                                          |
| `{ ... }`                               | `BlockNode`             | An **anonymous** container, often for arrays. Its `Identifier` is the `{` token. |
| `width = 1280` or `min_width <= 640`    | `ContentNode`           | A key, a separator, and a value. The fundamental data entry.                     |
| `stockholm` (inside a block)            | `KeyOnlyNode`           | A key without a value, typically used in a list.                                 |
| `scripted_trigger name = { ... }`       | `ScriptedStatementNode` | A special statement with a keyword (`scripted_trigger`) and a name.              |

---

#### Value Nodes (Things on the right side of a `=`)

| Syntax Example                                | Returns AST Node     | Description                                                               |
|:----------------------------------------------|:---------------------|:--------------------------------------------------------------------------|
| `1280`, `"text"`, `yes`, `high`, `1444.11.11` | `LiteralValueNode`   | A simple, single-token value (Number, String, Boolean, Identifier, Date). |
| `-10`                                         | `UnaryNode`          | An operator (`-`) applied to another value node.                          |
| `rgb { 0 0 0 }`                               | `FunctionCallNode`   | A named function with a list of value arguments.                          |
| `@[ 2 * 3 ]`                                  | `MathExpressionNode` | A container holding the unevaluated tokens of a math expression.          |
| `OR = { key = value }`                        | `BlockValueNode`     | A block that appears **as a value** on the right side of an assignment.   |