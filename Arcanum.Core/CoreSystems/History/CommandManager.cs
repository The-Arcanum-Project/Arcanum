using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.GameObjects.BaseTypes;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.History;

public static class CommandManager
{
    public static bool AddNexusDummyCommand(IEu5Object eu5Object, Enum attribute, object value)
    {
        var manager = AppData.HistoryManager;
        var command = manager.CurrentCommand;
        if (command is SetValueCommand dummyCommand)
        {
            if (dummyCommand.TryAdd(eu5Object, attribute, value))
                return true;
            
            dummyCommand.FinalizeSetup();
        }
        var newCommand = new SetValueCommand(eu5Object, attribute, value);
        manager.AddCommand(newCommand);
        return true;
    }
}