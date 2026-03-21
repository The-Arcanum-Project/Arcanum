using Arcanum.API.Console;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;

namespace Arcanum.Core.CoreSystems.ConsoleServices.Command_Implementations.Release_Commands;

public static class DuplicateOwnerCheckCommand
{
   private static DefaultCommands.DefaultCommandDefinition CreateOwner_DuplciationCommand()
   {
      const string usage =
         "owner_duplication  [group] | Checks all locations to see if they are assigned to multiple countries in collections which will make it consider owner. group: whether to group all collisions by owner instead of location.";

      return new(name: "owner_duplication ",
                 usage: usage,
                 execute: args =>
                 {
                    // Optional/Dynamic: group
                    var group = false;
                    if (args.Length > 0)
                       if (!args[0].StartsWith('-'))
                       {
                          if (!bool.TryParse(args[0], out var temp_group))
                             return ["Error: Argument 'group' must be true/false."];

                          group = temp_group;
                       }

                    List<string> results = [];
                    List<Country> owners = [];
                    HashSet<Country> groups = [];

                    foreach (var location in Globals.Locations.Values)
                    {
                       owners.Clear();
                       foreach (var country in Globals.Countries.Values)
                          if (country.OwnControlCores.Contains(location) ||
                              country.OwnColony.Contains(location) ||
                              country.OwnConquered.Contains(location) ||
                              country.OwnCores.Contains(location) ||
                              country.OwnIntegrated.Contains(location) ||
                              country.OwnControlConquered.Contains(location) ||
                              country.OwnControlColony.Contains(location) ||
                              country.OwnControlIntegrated.Contains(location))
                             owners.Add(country);

                       if (owners.Count > 1)
                       {
                          if (group)
                          {
                             foreach (var owner in owners)
                                groups.Add(owner);
                          }
                          else
                          {
                             results.Add($"Location '{location.UniqueId}' is assigned to multiple countries:");
                             foreach (var owner in owners)
                                results.Add($" - {owner.UniqueId}");
                          }
                       }
                    }

                    if (group)
                    {
                       results.Add("Countries that have duplicate ownerships:");
                       foreach (var groupOwner in groups)
                          results.Add($" - {groupOwner.UniqueId}");
                    }

                    return results.Count > 0 ? results.ToArray() : ["No duplicate owners found."];
                 },
                 clearance: ClearanceLevel.User,
                 category: DefaultCommands.CommandCategory.StandardUser,
                 aliases: ["od"]);
   }

   public static void RegisterCommands(ConsoleServiceImpl consoleServiceImpl)
   {
      consoleServiceImpl.RegisterCommand(CreateOwner_DuplciationCommand());
   }
}