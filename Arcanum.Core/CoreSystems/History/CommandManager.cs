using Arcanum.Core.CoreSystems.History.Commands;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.History;

public static class CommandManager
{
   public static bool AddNexusDummyCommand(IEu5Object eu5Object, Enum attribute)
   {
      var manager = AppData.HistoryManager;
      var command = manager.CurrentCommand;
      if (command is DummyChangeCommand dummyCommand)
      {
         if (dummyCommand.TryAdd(eu5Object, attribute))
            return true;

         // TODO: @MelCo an issue arises how to call execute before finalizing setup so it can be called after AddCommand too.
         dummyCommand.FinalizeSetup();
         dummyCommand.Execute();
      }

      var newCommand = new DummyChangeCommand(eu5Object, attribute);
      manager.AddCommand(newCommand);
      return true;
   }
}