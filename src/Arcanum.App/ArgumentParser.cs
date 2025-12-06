using Common.Logger;

namespace Arcanum.App;

public static class ArgumentParser
{
   internal static StartupOptions ParseArguments(string[] args)
   {
      // Supported Options and how to use them:
      // --headless / -h               : Run in headless mode (no UI)
      // --clean / -c                  : Clean temporary files on startup
      // --mod / -m <path>             : Specify the directory to load
      // --vanilla / -v <path>         : Specify a vanilla directory to include

      var options = new StartupOptions();
      string? currentCommand = null;

      foreach (var arg in args)
      {
         if (arg.StartsWith('-'))
         {
            currentCommand = arg.ToLowerInvariant();

            // Handle Boolean Flags immediately
            if (currentCommand is "--headless" or "-h" or "-batch")
               options.IsHeadless = true;
            else if (currentCommand is "--clean" or "-c")
               options.Clean = true;

            continue; // Move to next arg, which might be a value
         }

         // Process Values based on the active command
         switch (currentCommand)
         {
            case "--mod":
            case "-m":
               options.ModPath = arg;
               break;

            case "--vanilla":
            case "-v":
               options.BaseMods.Add(arg);
               break;

            // If we hit a value without a flag (or after a bool flag), ignore or log warning
            default:
               ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, $"Ignored argument: {arg}");
               ArcLog.WriteLine(CommonLogSource.PRT, LogLevel.INF, "Valid flags are --headless/-h, --mod/-m <path>, --vanilla/-v <path>");
               break;
         }
      }

      return options;
   }
}