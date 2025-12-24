using Arcanum.API.Console;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;

namespace Arcanum.Core.CoreSystems.ConsoleServices.Command_Implementations.Release_Commands;

internal static class DataReturnCommands
{
   private static DefaultCommands.DefaultCommandDefinition CreateCopypropertiesCommand()
   {
      const string usage = "CopyProperties <type> | Copies all properties of given type type: the name of the targeted type.";

      return new(name: "copyproperties",
                 usage: usage,
                 execute: args =>
                 {
                    if (args.Length < 1)
                       return ["Usage: " + usage];

                    var type = string.Join(" ", args[..]);
                    var fType = EmptyRegistry.Empties.Keys.FirstOrDefault(x => x.Name == type);
                    if (fType == null)
                       return [$"Error: Type '{type}' not found in registry."];

                    var empty = EmptyRegistry.Empties[fType];
                    if (empty is not IEu5Object eu5Object)
                       return [$"Error: Type '{type}' does not implement IEu5Object."];

                    return eu5Object.GetAllProperties().Select(x => x.ToString()).ToArray();
                 },
                 clearance: ClearanceLevel.User,
                 category: DefaultCommands.CommandCategory.StandardUser,
                 aliases: ["cp"]);
   }

   public static void RegisterCommands(ConsoleServiceImpl consoleServiceImpl)
   {
      consoleServiceImpl.RegisterCommand(CreateCopypropertiesCommand());
   }
}