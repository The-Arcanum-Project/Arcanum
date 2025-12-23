using System.IO;
using Arcanum.API.Console;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Tracing;
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
                    if (argument.StartsWith('-'))
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

   private static DefaultCommands.DefaultCommandDefinition CreateMapTracingValidatorCommand()
   {
      const string usage = "maptrace-validator | Validates map parsing and generates data to compare.";

      return new(name: "maptrace-validator",
                 usage: usage,
                 execute: args =>
                 {
                    if (args.Length != 0)
                       return ["Usage: " + usage];

                    Bitmap bmmpp = new(DescriptorDefinitions.MapTracingDescriptor.Files[0].Path.FullPath);
                    using var tracer = new MapTracing(bmmpp);
                    var result = tracer.Trace();

                    var hashAfter = MapTracingValidator.GenerateFingerprint(result);

                    var baseLineFile = Path.Combine(IO.IO.GetLogsPath, "baseline_hash.txt");
                    if (!File.Exists(baseLineFile))
                    {
                       IO.IO.WriteAllTextUtf8(baseLineFile, hashAfter);
                       return ["Baseline hash file created at:", baseLineFile, "Please run the validator again to compare."];
                    }

                    var hashBefore = File.ReadAllText(baseLineFile);
                    string res;

                    if (hashAfter == hashBefore)
                       res = "SUCCESS: Optimization Validated. 100% Match.";
                    else
                    {
                       res = "FAILURE: Output mismatch.";
                       MapTracingValidator.DumpToFile(result, Path.Combine(IO.IO.GetLogsPath, "optimized_dump.txt"));
                       MapTracingValidator.RenderDebugImage(result, bmmpp.Width, bmmpp.Height, Path.Combine(IO.IO.GetLogsPath, "optimized_vis.png"));
                    }

                    return [res];
                 },
                 clearance: ClearanceLevel.User,
                 category: DefaultCommands.CommandCategory.StandardUser,
                 aliases: ["mp"]);
   }

   public static void RegisterCommands(ConsoleServiceImpl consoleServiceImpl)
   {
      consoleServiceImpl.RegisterCommand(CreateValidatorCommand());
      consoleServiceImpl.RegisterCommand(CreateMapTracingValidatorCommand());
   }
}