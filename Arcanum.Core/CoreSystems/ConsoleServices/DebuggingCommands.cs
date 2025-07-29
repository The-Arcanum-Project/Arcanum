using Arcanum.API.Console;

namespace Arcanum.Core.CoreSystems.ConsoleServices;

public static class DebuggingCommands
{
   public static ICommandDefinition CreateSearchCommand()
   {
      const string usage = "search <query> | Searches using the Queastor system for the given query.";
      return new DefaultCommands.DefaultCommandDefinition(name: "search",
                                                          usage: usage,
                                                          execute: args =>
                                                          {
                                                             if (args.Length != 1)
                                                                return [usage];

                                                             var results =
                                                                Queastor.Queastor.GlobalInstance.Search(args[0]);
                                                             if (results.Count == 0)
                                                                return ["No results found."];

                                                             return results
                                                                   .Select(result
                                                                              => $"- {result.GetNamespace} : {result.ResultName}")
                                                                   .ToArray();
                                                          },
                                                          clearance: ClearanceLevel.User,
                                                          category: DefaultCommands.CommandCategory
                                                            .Debug, // Assign Category
                                                          aliases: []);
   }

   public static ICommandDefinition CreateSearchExeCommand()
   {
      const string usage =
         "search_exe <query> | Searches using the Queastor and executes the selection of the first result";
      return new DefaultCommands.DefaultCommandDefinition(name: "search_exe",
                                                          usage: usage,
                                                          execute: args =>
                                                          {
                                                             if (args.Length != 1)
                                                                return [usage];

                                                             var results =
                                                                Queastor.Queastor.GlobalInstance.Search(args[0]);
                                                             if (results.Count == 0)
                                                                return ["No results found."];

                                                             var firstResult = results.First();
                                                             firstResult.OnSearchSelected();
                                                             return [$"Executed: {firstResult.GetNamespace}"];
                                                          },
                                                          clearance: ClearanceLevel.User,
                                                          category: DefaultCommands.CommandCategory
                                                            .Debug, // Assign Category
                                                          aliases: []);
   }

   
}