using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;

namespace Arcanum.Core.CoreSystems.ConsoleServices.Command_Implementations.Release_Commands;

public static class ValidatorCommands
{
   private static DefaultCommands.DefaultCommandDefinition CreateValidatorCommand()
   {
      const string usage = "validator -l | validator -\"<validator>\" | Validates inputs or lists available validators.";

      return new(name: "validator",
                 usage: usage,
                 execute: args =>
                 {
                    if (args.Length == 0)
                       return ["Usage: " + usage];

                    var argument = args[0];

                    // List Mode (-l)
                    if (argument.Equals("-l", StringComparison.OrdinalIgnoreCase))
                       return ["Available Validators:", ..ParsingMaster.Validators.Select(x => $"- {x.Name}")];

                    // Validation Mode (--<string>)
                    if (argument.StartsWith("-"))
                    {
                       var payload = argument[1..];

                       if (string.IsNullOrWhiteSpace(payload))
                          return ["Error: No input string provided after '-'."];

                       var validator = ParsingMaster.Validators
                                                    .FirstOrDefault(v => v.Name.Equals(payload, StringComparison.OrdinalIgnoreCase));
                       if (validator == null)
                          return [$"Error: Validator '{payload}' not found. Use 'validator -l' to list available validators."];

                       ErrorManager.Diagnostics.Clear();
                       validator.Validate();

                       return [$"Validation complete using '{validator.Name}' validator. Check error log for any warnings or errors."];
                    }

                    // Fallback for unrecognized arguments
                    return ["Unknown argument/flag. Usage: " + usage];
                 },
                 clearance: ClearanceLevel.User,
                 category: DefaultCommands.CommandCategory.StandardUser,
                 aliases: []);
   }

   public static void RegisterCommands(ConsoleServiceImpl consoleServiceImpl)
   {
      consoleServiceImpl.RegisterCommand(CreateValidatorCommand());
   }
}