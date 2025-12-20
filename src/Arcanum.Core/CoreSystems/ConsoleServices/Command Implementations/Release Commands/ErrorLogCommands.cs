using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.ErrorSystem;

namespace Arcanum.Core.CoreSystems.ConsoleServices.Command_Implementations.Release_Commands;

public static class ErrorLogCommands
{
   private static DefaultCommands.DefaultCommandDefinition CreateClearErrorsCommand()
   {
      const string usage = "clear_errors | Clears the internal error log.";

      return new(name: "clear_errors",
                 usage: usage,
                 execute: _ =>
                 {
                    var count = ErrorManager.Diagnostics.Count;
                    ErrorManager.Diagnostics.Clear();

                    return ["Cleared " + count + " error log entries."];
                 },
                 clearance: ClearanceLevel.User,
                 category: DefaultCommands.CommandCategory.StandardUser,
                 aliases: ["ce"]);
   }

   public static void RegisterCommands(ConsoleServiceImpl consoleServiceImpl)
   {
      consoleServiceImpl.RegisterCommand(CreateClearErrorsCommand());
   }
}