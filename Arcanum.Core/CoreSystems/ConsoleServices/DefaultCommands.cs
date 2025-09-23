using System.IO;
using System.Text;
using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Common.UI;

namespace Arcanum.Core.CoreSystems.ConsoleServices;

public static class DefaultCommands
{
   [Flags]
   public enum CommandCategory
   {
      None = 0, // No commands
      Basic = 1 << 0, // echo, help, clear, list...
      Alias = 1 << 1, // alias command...
      Macro = 1 << 2, // macro command...
      History = 1 << 3, // history command...
      FileSystem = 1 << 4, // pwd...
      Debug = 1 << 6, // set clearance (and other debug-specific commands)

      // Common combinations
      StandardUser = Basic | Alias | Macro | History | FileSystem,

      All = ~None, // Special value to include all defined categories (bitwise NOT of None)
   }

   public class DefaultCommandDefinition : CommandBase
   {
      public CommandCategory Category { get; set; }

      public DefaultCommandDefinition(string name,
                                      string usage,
                                      Func<string[], string[]> execute,
                                      ClearanceLevel clearance,
                                      CommandCategory category,
                                      params string[] aliases) : base(name, usage, clearance, aliases, execute)
      {
         Category = category;
      }
   }

   public static ICommandDefinition CreateHelpCommand(IConsoleService consoleService)
   {
      const string usage =
         "help [command_name] | Displays help for command x";
      return new DefaultCommandDefinition(name: "help",
                                          usage: usage,
                                          execute: args =>
                                          {
                                             if (args.Length != 1 ||
                                                 !consoleService.GetCommandDefinition(args[0], out var commandDef))
                                                return ["Usage: " + usage];

                                             var aliasString = commandDef!.Aliases.Count > 0
                                                                  ? $"aliases: {string.Join(", ", commandDef.Aliases)}"
                                                                  : "No aliases defined.";

                                             return [commandDef.Usage, aliasString];
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Basic);
   }

   public static ICommandDefinition CreateEchoCommand()
   {
      return new DefaultCommandDefinition(name: "echo",
                                          usage: "echo <message ...> | Displays the provided message.",
                                          execute: args => [string.Join(" ", args)],
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Basic,
                                          aliases: ["say"]);
   }

   public static ICommandDefinition CreateClearCommand(IConsoleService consoleService)
   {
      return new DefaultCommandDefinition(name: "clear",
                                          usage: "clear | Clears the console output.",
                                          execute: _ =>
                                          {
                                             consoleService.Clear();
                                             return [];
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Basic,
                                          aliases: ["cls"]);
   }

   public static ICommandDefinition CreateListCommandsCommand(IConsoleService consoleService)
   {
      return new DefaultCommandDefinition(name: "list",
                                          usage: "list | Lists all available commands.",
                                          execute: _ =>
                                          {
                                             var cmds = consoleService.GetRegisteredCommandsWithoutAliases();
                                             var lines = new string[cmds.Count + 1];
                                             lines[0] = "Available Commands:";
                                             StringBuilder sb = new();
                                             for (var i = 0; i < cmds.Count; i++)
                                             {
                                                var cmd = cmds[i];
                                                switch (cmd.Clearance)
                                                {
                                                   case ClearanceLevel.User:
                                                      sb.Append("[USR]");
                                                      break;
                                                   case ClearanceLevel.Admin:
                                                      sb.Append("[ADM]");
                                                      break;
                                                   case ClearanceLevel.Debug:
                                                      sb.Append("[DBG]");
                                                      break;
                                                   default:
                                                      throw new ArgumentOutOfRangeException();
                                                }

                                                sb.Append($" | {cmd.Name.PadRight(20)} | {cmd.Usage}");

                                                lines[i + 1] = sb.ToString();
                                                sb.Clear();
                                             }

                                             return lines.Length > 0
                                                       ? lines
                                                       : ["No commands registered."];
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Basic);
   }

   public static ICommandDefinition CreateAliasCommand(IConsoleService consoleService)
   {
      return new DefaultCommandDefinition(name: "alias",
                                          usage: "alias <name> <command_to_alias> alias -r/-c <name>",
                                          execute: args =>
                                          {
                                             if (args.Length != 2)
                                                return ["Usage: alias <name> <command_to_alias> | alias -r/-c <name>"];

                                             switch (args[0])
                                             {
                                                case "-r":
                                                   if (consoleService.RemoveAlias(args[1]))
                                                      return [$"Alias '{args[1]}' removed."];

                                                   return [$"Alias '{args[1]}' not found."];
                                                case "-c":
                                                   foreach (var alias in consoleService.GetCommandAliases())
                                                      consoleService.RemoveAlias(alias);
                                                   return ["All aliases cleared."];
                                                default:
                                                   consoleService.SetAlias(args[0], args[1]);
                                                   return [$"Alias '{args[0]}' added for command: {args[1]}"];
                                             }
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Alias);
   }

   public static ICommandDefinition CreateMacroCommand(IConsoleService consoleService)
   {
      const string usage = "macro <name> \"<commands>\" | macro -r/-l/-c <name> | macro <name>";
      return new DefaultCommandDefinition(name: "macro",
                                          usage: usage,
                                          execute: args =>
                                          {
                                             switch (args.Length)
                                             {
                                                case 1:
                                                   switch (args[0])
                                                   {
                                                      case "-l":
                                                         return consoleService.GetMacros()
                                                                              .Select(kvp => $"{kvp.Key}: {kvp.Value}")
                                                                              .ToArray();
                                                      case "-c":
                                                         consoleService.ClearMacros();
                                                         return ["All macros cleared."];
                                                      default:
                                                         consoleService.RunMacro(args[0], out var output);
                                                         return output;
                                                   }
                                                case 2:
                                                   switch (args[0])
                                                   {
                                                      case "-r":
                                                         if (consoleService.RemoveMacro(args[1]))
                                                            return [$"Macro '{args[1]}' removed."];

                                                         return [$"Macro '{args[1]}' not found."];
                                                      default:
                                                         consoleService.AddMacro(args[0], args[1]);
                                                         return [$"Macro '{args[0]}' added with commands: {args[1]}"];
                                                   }
                                             }

                                             return [usage];
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Macro);
   }

   public static ICommandDefinition CreateHistoryCommand(IConsoleService consoleService)
   {
      return new DefaultCommandDefinition(name: "history",
                                          usage: "history [-c] | Shows command history or clears it with -c.",
                                          execute: args =>
                                          {
                                             if (args is not ["-c"])
                                                return args.Length > 0
                                                          ?
                                                          [
                                                             "Invalid argument for history command. Use -c to clear history."
                                                          ]
                                                          : consoleService.GetHistory().ToArray();

                                             consoleService.ClearHistory();
                                             return ["Command history cleared."];
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.History);
   }

   public static ICommandDefinition CreatePwdCommand()
   {
      return new DefaultCommandDefinition(name: "pwd",
                                          usage: "pwd | Prints the current working directory of the application.",
                                          execute: _ => [Directory.GetCurrentDirectory()],
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.FileSystem,
                                          aliases: ["cwd", "dir"]);
   }

   public static ICommandDefinition CreateTableCommand()
   {
      return new DefaultCommandDefinition(name: "table",
                                          usage:
                                          "table <c1_v1,c1_v2,...> <c2_v1,c2_v2,...> [...] ",
                                          execute: args =>
                                          {
                                             if (args.Length == 0)
                                                return ["No columns provided for table."];

                                             var columns = ConsoleParser.GetSubArguments(args);
                                             return DrawTable(separator: '|', columns: columns);
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory.Basic);
   }

   public static ICommandDefinition CreateSetClearanceCommand(IConsoleService consoleService)
   {
      return new DefaultCommandDefinition(name: "setclearance",
                                          usage:
                                          "setclearance <User|Admin|Debug> | Sets the clearance level.",
                                          execute: _ => [consoleService.CurrentClearance.ToString()],
                                          clearance: ClearanceLevel.Admin,
                                          category: CommandCategory.Debug);
   }

   public static ICommandDefinition BrowseCommand()
   {
      List<string> supportedTypes = ["metadata", "mmsd"];

      var usage =
         $"browse <{string.Join('|', supportedTypes)}> | Opens the related object in a property browser.";

      return new DefaultCommandDefinition(name: "browse",
                                          usage: usage,
                                          execute: args =>
                                          {
                                             if (args.Length != 1)
                                                return [usage];

                                             var type = args[0].ToLowerInvariant();
                                             switch (type)
                                             {
                                                case "metadata":
                                                   var metadata = CoreData.ModMetadata;
                                                   UIHandle.Instance.PopUpHandle.OpenPropertyGridWindow(metadata);
                                                   break;
                                                case "mmsd":
                                                   UIHandle.Instance.PopUpHandle
                                                           .OpenPropertyGridWindow(AppData.MainMenuScreenDescriptor);
                                                   break;
                                                default:
                                                   return
                                                   [
                                                      $"Unknown type. Supported types: {string.Join(", ", supportedTypes)}"
                                                   ];
                                             }

                                             return ["Finished opening AppData in property browser."];
                                          },
                                          clearance: ClearanceLevel.User,
                                          category: CommandCategory
                                            .StandardUser,
                                          aliases: []);
   }

   public static ICommandDefinition PrintLoadingTimesCommand()
   {
      var usage = "Prints the loading times of the application.";

      return new DefaultCommandDefinition(name: "printLT",
                                          usage: usage,
                                          execute: _ =>
                                          {
                                             var output = new List<string> { "Loading Times:" };
                                             foreach (var (key, value) in ParsingMaster.StepDurationsByName)
                                                output.Add($"{key,-25}: {value.TotalMilliseconds,8:#####.0} ms");

                                             return output.ToArray();
                                          },
                                          clearance: ClearanceLevel.Debug,
                                          category: CommandCategory.Debug);
   }

   private static string[] DrawTable(char separator = '|', params string[][] columns)
   {
      if (columns.Length == 0)
         return ["No columns provided for table."];

      var rowCount = columns[0].Length;

      if (columns.Any(col => col.Length != rowCount))
         return ["Error: All columns must have the same number of rows."];

      var colWidths = columns.Select(col => col.Max(cell => cell.Length)).ToArray();
      var output = new string[rowCount];

      for (var row = 0; row < rowCount; row++)
      {
         var rowBuilder = new StringBuilder();
         for (var colIdx = 0; colIdx < columns.Length; colIdx++)
         {
            rowBuilder.Append((columns[colIdx][row]).PadRight(colWidths[colIdx]));
            if (colIdx < columns.Length - 1)
               rowBuilder.Append($" {separator} ");
         }

         output[row] = rowBuilder.ToString();
      }

      return output;
   }

   // --- Method to get all defined default commands ---
   private static IEnumerable<ICommandDefinition> GetAllDefaultCommandDefinitions(
      IConsoleService consoleService)
   {
      yield return CreateHelpCommand(consoleService);
      yield return CreateEchoCommand();
      yield return CreateClearCommand(consoleService);
      yield return CreateListCommandsCommand(consoleService);
      yield return CreateAliasCommand(consoleService);
      yield return CreateMacroCommand(consoleService);
      yield return CreateHistoryCommand(consoleService);
      yield return CreatePwdCommand();
      yield return CreateTableCommand();
      yield return CreateSetClearanceCommand(consoleService);
      yield return DebuggingCommands.CreateSearchCommand();
      yield return DebuggingCommands.CreateSearchExeCommand();
      yield return DebuggingCommands.PrintQueastorStatsCommand();
      yield return BrowseCommand();
      yield return PrintLoadingTimesCommand();
   }

   public static IEnumerable<ICommandDefinition> GetDefaultCommands(
      CommandCategory categories,
      IConsoleService consoleService)
   {
      foreach (var cmdDef in GetAllDefaultCommandDefinitions(consoleService))
         // Check if the command's Category is included in the requested categories
         // This requires DefaultCommandDefinition to expose its Category.
         if (cmdDef is DefaultCommandDefinition defaultCmd && (defaultCmd.Category & categories) != 0)
            yield return defaultCmd;
   }

   public static void RegisterDefaultCommands(
      IConsoleService consoleService,
      CommandCategory categoriesToRegister)
   {
      if (categoriesToRegister == CommandCategory.None)
         return;

      var commandsToRegister = GetDefaultCommands(categoriesToRegister, consoleService);
      foreach (var command in commandsToRegister)
         consoleService.RegisterCommand(command);
   }
}