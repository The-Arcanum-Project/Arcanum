# Arcanum Console System Documentation

The Arcanum Console is a flexible, service-based command-line interface integrated into the application. It supports
command execution, argument parsing, history navigation, macros, aliasing, and permission-based access control.

## Part 1: Developer Guide

This section explains how to define new commands and register them with the `IConsoleService`.

### 1. The Command Interface

All commands must implement the `ICommandDefinition` interface. However, the easiest way to create a command is to
inherit from the `CommandBase` abstract class, which handles boilerplate code like equality checks.

**Properties of a Command:**

* **Name:** The keyword used to invoke the command (case-insensitive).
* **Usage:** A help string explaining how to use the command.
* **Clearance:** The `ClearanceLevel` required to execute it (`User`, `Admin`, or `Debug`).
* **Aliases:** A list of alternative keywords for the command.
* **Execute:** A function that takes string arguments and returns an array of output strings.

### 2. Creating a Command

#### Approach A: Class-Based (Recommended for complex logic)

Create a class that inherits from `CommandBase`.

```csharp
using Arcanum.API.Console;

public class TeleportCommand : CommandBase
{
    public TeleportCommand() : base(
        name: "teleport",
        usage: "teleport <x> <y> <z> | Teleports the player to coordinates.",
        clearance: ClearanceLevel.Admin, // Only Admins can use this
        aliases: new[] { "tp", "warp" },
        execute: ExecuteLogic
    ) {}

    private static string[] ExecuteLogic(string[] args)
    {
        // 1. Validate Arguments
        if (args.Length != 3)
            return new[] { "Error: Usage is teleport <x> <y> <z>" };

        // 2. Perform Logic
        if (float.TryParse(args[0], out float x) && 
            float.TryParse(args[1], out float y) && 
            float.TryParse(args[2], out float z))
        {
            // Call your game logic here...
            // Player.MoveTo(x, y, z);
            
            return new[] { $"Teleported to {x}, {y}, {z}" };
        }

        return new[] { "Error: Coordinates must be numbers." };
    }
}
```

#### Approach B: Inline Definition (Recommended for simple tools)

You can define commands on the fly using a standard implementation (like the one found in `DefaultCommands`).

```csharp
var myCommand = new DefaultCommands.DefaultCommandDefinition(
    name: "greet",
    usage: "greet <name>",
    execute: args => 
    {
        if (args.Length == 0) return new[] { "Hello, World!" };
        return new[] { $"Hello, {args[0]}!" };
    },
    clearance: ClearanceLevel.User,
    category: DefaultCommands.CommandCategory.Basic,
    aliases: new[] { "hi" }
);
```

### 3. Registering the Command

Once a command is defined, it must be registered with the `IConsoleService` instance.

```csharp
public void Initialize(IConsoleService consoleService)
{
    // Register a class-based command
    consoleService.RegisterCommand(new TeleportCommand());

    // Register an inline command
    consoleService.RegisterCommand(myCommand);
}
```

### 4. Handling Output

The `Execute` function expects a return type of `string[]`.

* Each string in the array represents a new line in the console output.
* Return an empty array `[]` if the command succeeds silently.
* Return error messages starting with "Error:".

### 5. Argument Parsing

The console automatically handles argument splitting.

* **Spaces** divide arguments.
* **Quotes (`""`)** group arguments containing spaces.

**Example:**
Input: `give_item "Iron Sword" 1`

* `args[0]`: `Iron Sword` (Quotes are stripped by default)
* `args[1]`: `1`

---

## Part 2: User Guide

This section explains how to use the console window during runtime.

### 1. Basic Usage

* **Open Console:** (Depending on keybinding, usually `~` or `F1`).
* **Execute:** Type a command and press `Enter`.
* **Navigation:**
    * `Up Arrow`: Scroll back through command history.
    * `Down Arrow`: Scroll forward through command history.
* **Prefix:** The console line starts with ` > `. You cannot delete this prefix.

### 2. Built-in Commands

| Command     | Usage                   | Description                                                                           |
|:------------|:------------------------|:--------------------------------------------------------------------------------------|
| **help**    | `help [command]`        | Displays usage info. If a command name is provided, shows specific details.           |
| **list**    | `list`                  | Lists all registered commands and their clearance levels (`[USR]`, `[ADM]`, `[DBG]`). |
| **clear**   | `clear` (or `cls`)      | Wipes the console output window.                                                      |
| **echo**    | `echo <message>`        | Prints the text back to the console. Useful for testing macros.                       |
| **pwd**     | `pwd`                   | Prints the application's current working directory.                                   |
| **history** | `history [-c]`          | Shows past commands. Use `-c` to clear the history.                                   |
| **table**   | `table <c1,c2> <d1,d2>` | Draws a formatted text table based on comma-separated arguments.                      |

### 3. Advanced Features

#### Aliases

You can create short custom names for existing commands.

* **Create Alias:** `alias <new_name> <existing_command>`
    * *Example:* `alias s search` (Now typing `s query` works like `search query`).
* **Remove Alias:** `alias -r <alias_name>`
* **Clear All:** `alias -c`

#### Macros

Macros allow you to save a specific command string (including arguments) to a keyword. Macros are saved to disk (
`ConsoleData/macros.json`) and persist between sessions.

* **Create Macro:** `macro <key> "<value>"`
    * *Example:* `macro spawn_loadout "give_item sword 1"`
    * *Usage:* Type `spawn_loadout` and press Enter to execute the stored command.
* **List Macros:** `macro -l`
* **Remove Macro:** `macro -r <key>`
* **Clear Macros:** `macro -c`

#### Debugging & Search (Queastor)

If the Debug module is enabled:

* **search**: `search <query>` - Searches the Queastor system for objects.
* **search_exe**: `search_exe <query>` - Searches and automatically executes/selects the first result.
* **printLT**: Prints application loading times.
* **browse**: `browse metadata` or `browse mmsd` - Opens a property grid window for internal data.

### 4. Error Messages

* **"Unknown command"**: Check your spelling or type `list` to see what is available.
* **"Permission denied"**: The command requires a higher clearance level (Admin/Debug) than your current session has.
* **"No output returned"**: The command executed successfully but had nothing to say.

---

## Part 3: Architecture Overview (For Maintainers)

### Key Components

1. **`ConsoleServiceImpl`**: The brain. It manages the dictionary of commands, handles input string parsing (splitting
   quotes), and maintains the History list. It saves/loads state using `IJsonProcessor` and `IFileOperations`.
2. **`ConsoleWindow` (UI)**: A WPF window acting as an `IOutputReceiver`. It handles key inputs (Up/Down/Enter) and
   renders text to a TextBox.
3. **`ClearanceLevel`**:
    * `User`: Standard commands.
    * `Admin`: Administrative commands (state changes).
    * `Debug`: Developer tools (object browsers, stats).

### Persistence

The console stores data in the `ConsoleData` folder:

* `{Identifier}_macros.json`: User-defined macros.
* `{Identifier}_consoleHistory.json`: The session command history.

### Adding a new UI Frontend

If you want to render the console in a game engine (like Unity or Unreal) instead of WPF:

1. Implement `IOutputReceiver`.
2. Pass that receiver to `consoleService.SetOutputReciever(myGameConsole)`.
3. Forward input strings from your game engine GUI to `consoleService.ProcessCommand(input)`.