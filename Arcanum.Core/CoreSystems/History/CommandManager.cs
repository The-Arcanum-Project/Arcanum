using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.CoreSystems.History.Commands.Collections;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.History;

public static class CommandManager
{
    /// <summary>
    /// Creates a new SetValueCommand or merges with the current one if possible.
    /// </summary>
    public static bool SetValueCommand(IEu5Object eu5Object, Enum attribute, object value)
    {
        // Call the generic handler with the specific logic for SetValueCommand
        return HandleCommand(
            specificCommand => specificCommand.TryAdd(eu5Object, attribute, value),
            () => new SetValueCommand(eu5Object, attribute, value)
        );
    }

    /// <summary>
    /// Creates a new ClearCollectionCommand or merges with the current one if possible.
    /// </summary>
    public static bool ClearCollectionCommand(IEu5Object eu5Object, Enum attribute)
    {
        // Call the generic handler with the specific logic for ClearCollectionCommand
        return HandleCommand(
            specificCommand => specificCommand.TryAdd(eu5Object, attribute),
            () => new ClearCollectionCommand(eu5Object, attribute)
        );
    }
    
    public static bool RemoveFromCollectionCommand(IEu5Object eu5Object, Enum attribute, object value)
    {
        return HandleCommand(
            specificCommand => specificCommand.TryAdd(eu5Object, attribute, value),
            () => new RemoveFromCollectionCommand(eu5Object, attribute, value)
        );
    }
    
    public static bool AddToCollectionCommand(IEu5Object eu5Object, Enum attribute, object value)
    {
        return HandleCommand(
            specificCommand => specificCommand.TryAdd(eu5Object, attribute, value),
            () => new AddToCollectionCommand(eu5Object, attribute, value)
        );
    }

    /// <summary>
    /// A generic method to handle the creation and merging of history commands.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to handle, must be an Eu5ObjectCommand.</typeparam>
    /// <param name="tryAddAction">A function that attempts to merge the operation with an existing command.</param>
    /// <param name="createCommandAction">A function that creates a new command if merging is not possible.</param>
    /// <returns>Always returns true.</returns>
    private static bool HandleCommand<TCommand>(Func<TCommand, bool> tryAddAction, Func<TCommand> createCommandAction) where TCommand : Eu5ObjectCommand
    {
        var manager = AppData.HistoryManager;
        var command = manager.CurrentCommand;

        if (command is Eu5ObjectCommand eu5Command)
        {
            // Check if the current command is of the specific type we want and if we can add to it.
            if (eu5Command is TCommand specificCommand && tryAddAction(specificCommand))
            {
                return true;
            }
            
            // If we can't merge, finalize the previous command.
            eu5Command.FinalizeSetup();
        }

        // Create and add the new command.
        var newCommand = createCommandAction();
        manager.AddCommand(newCommand);
        return true;
    }
    
    /* If performance becomes an issue, we can revert to the original non-generic methods.
    public static bool SetValueCommand(IEu5Object eu5Object, Enum attribute, object value)
    {
        var manager = AppData.HistoryManager;
        var command = manager.CurrentCommand;
        if (command is Eu5ObjectCommand eu5Command)
        {
            if (eu5Command is SetValueCommand specificCommand && specificCommand.TryAdd(eu5Object, attribute, value))
                return true;

            eu5Command.FinalizeSetup();
        }
        var newCommand = new SetValueCommand(eu5Object, attribute, value);
        manager.AddCommand(newCommand);
        return true;
    }
    public static bool ClearCollectionCommand(IEu5Object eu5Object, Enum attribute)
    {
        var manager = AppData.HistoryManager;
        var command = manager.CurrentCommand;
        if (command is Eu5ObjectCommand eu5Command)
        {
            if (eu5Command is ClearCollectionCommand specificCommand && specificCommand.TryAdd(eu5Object, attribute))
                return true;

            eu5Command.FinalizeSetup();
        }
        var newCommand = new ClearCollectionCommand(eu5Object, attribute);
        manager.AddCommand(newCommand);
        return true;
    }
    */
}