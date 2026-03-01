# Arcanum Command System

A "Single Source of Truth" command infrastructure for WPF that eliminates view-layer duplication.
This system shifts the responsibility of metadata, shortcuts, and scoping from XAML into strongly-typed command objects.

## Key Features

- **Self-Documenting:** Commands carry their own `DisplayName`, `Description`, `Category`, and `Gestures`.
- **Type-Safe IDs:** Hierarchical, compiler-checked IDs (e.g., `CommandIds.UI.Window.Close`).
- **Auto-Generated Tooltips:** Tooltips automatically update to include current shortcut hints: `Close (Esc)`.
- **Contextual Scoping:** Bindings are automatically injected into Windows or UserControls based on defined scopes (
  `Global`, `Dialog`, `Editor`, etc.).
- **Zero-Boilerplate XAML:** Bind `Command`, `Content`, `ToolTip` and for dialogs `CommandParameter` in a single line.

---

## Implementation Guide

### 1. Define the Identity

Create hierarchical IDs using the `CommandId` record. This ensures naming consistency across the entire application.

```csharp
public static class CommandIds {
    public static class UI {
        private const string PATH = nameof(UI);
        public static class Window {
            private const string WINDOW_PATH = $"{UI.PATH}.{nameof(Window)}";
            public static readonly CommandId Close = CommandId.Create(WINDOW_PATH);
        }
    }
}
```

### 2. Register Logic in the Library

Centralize your execution logic and default shortcuts in a `CommandLibrary`.

```csharp
public static void Initialize() {
    new ManagedCommand(
        id:          CommandIds.UI.Window.Close,
        name:        "Close",
        description: "Closes the current window.",
        category:    "Window",
        scope:       CommandScopes.DIALOG,
        execute:     param => (param as Window)?.Close()
    ).WithDefaultGesture(Key.Escape);
}
```

### 3. Use in the View (XAML)

Set the `Scopes` on the Window to activate relevant shortcuts, and use `Assign` to wire up UI elements instantly.

```xaml
<Window ... 
        commands:CommandBinder.Scopes="Dialog"
        DataContext="{Binding MyViewModel}">

    <!-- Content, Command, ToolTip, and for dialogs Window-Parameter all set automatically -->
    <Button commands:CommandBinder.Assign="{Binding CloseCommand}" />

</Window>
```

---

## Architecture Overview

- **`IAppCommand`**: The interface extending `ICommand` to include metadata and `Gestures`.
- **`ManagedCommand`**: The concrete implementation that registers itself with the registry upon instantiation.
- **`CommandRegistry`**: The runtime database of all commands. Used for persistence (saving/loading shortcuts) and
  documentation generation.
- **`CommandBinder`**: The engine. An attached property system that synchronizes WPF `InputBindings` with the command
  registry and manages scope filtering.
