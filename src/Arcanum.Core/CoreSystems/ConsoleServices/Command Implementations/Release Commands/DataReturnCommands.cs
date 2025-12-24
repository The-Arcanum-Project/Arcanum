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

   private static DefaultCommands.DefaultCommandDefinition CreateList_Game_ObjectsCommand()
   {
      const string usage = "list_game_objects [-e] | Lists all available game objects -e: exclued embedded.";

      return new(name: "list_game_objects",
                 usage: usage,
                 execute: args =>
                 {
                    // Flag: -e
                    var e = args.Contains("e", StringComparer.OrdinalIgnoreCase);
                    var keys = e
                                  ? Eu5ObjectsRegistry.Eu5Objects.Where(x => !x.IsAssignableTo(typeof(IEmbeddedEu5Object<>))).Select(x => x.Name).ToArray()
                                  : Eu5ObjectsRegistry.Eu5Objects.Select(x => x.Name).ToArray();

                    // TODO: Implement Logic
                    return [$"Number of game objects: {keys.Length}", ..keys];
                 },
                 clearance: ClearanceLevel.User,
                 category: DefaultCommands.CommandCategory.StandardUser,
                 aliases: ["list_gos"]);
   }

   public static void RegisterCommands(ConsoleServiceImpl consoleServiceImpl)
   {
      consoleServiceImpl.RegisterCommand(CreateCopypropertiesCommand());
      consoleServiceImpl.RegisterCommand(CreateList_Game_ObjectsCommand());
   }
}