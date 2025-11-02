using System.Text;

namespace Arcanum.Core.CoreSystems.ConsoleServices;

public static class ConsoleParser
{
   /// <summary>
   /// Takes in the input arguments without the command name and returns the arguments
   /// </summary>
   /// <param name="inputArgs"></param>
   /// <param name="expected"></param>
   /// <returns></returns>
   public static bool GetCommandArguments(string[] inputArgs, int expected)
   {
      if (inputArgs.Length < expected)
         return false;

      for (var i = 0; i < expected; i++)
         if (string.IsNullOrWhiteSpace(inputArgs[i]))
            return false;

      return true;
   }

   /// <summary>
   /// Splits the provided arguments into sub-arguments based on the specified separator,
   /// while handling quoted sections to preserve enclosed separators.
   /// </summary>
   /// <param name="args">The array of input arguments to be processed.</param>
   /// <param name="separator">The character used to separate arguments. Defaults to a comma (,).</param>
   /// <returns>A jagged array where each inner array represents a group of sub-arguments.</returns>
   public static string[][] GetSubArguments(string[] args, char separator = ',')
   {
      var subArgs = new List<string[]>();
      var currentArgs = new List<string>();
      var inQuotes = false;
      var currentArg = new StringBuilder();

      foreach (var arg in args)
      {
         foreach (var c in arg)
            if (c == '"')
            {
               inQuotes = !inQuotes; // Toggle inQuotes state
            }
            else if (c == separator && !inQuotes)
            {
               if (currentArg.Length <= 0)
                  continue;

               currentArgs.Add(currentArg.ToString().Trim());
               currentArg.Clear();
            }
            else
            {
               currentArg.Append(c);
            }

         // Add the last argument if it exists
         if (currentArg.Length > 0)
         {
            currentArgs.Add(currentArg.ToString().Trim());
            currentArg.Clear();
         }

         subArgs.Add(currentArgs.ToArray());
         currentArgs.Clear();
      }

      return subArgs.ToArray();
   }

   public static string[] SplitStringQuotes(string cmd,
                                            char splitChar = ' ',
                                            char quoteChar = '"',
                                            bool trimQuotes = true)
   {
      List<string> parts = [];
      var inQuotes = false;
      var current = string.Empty;
      foreach (var c in cmd)
      {
         if (c == quoteChar)
         {
            inQuotes = !inQuotes;
            if (trimQuotes)
               continue;
         }

         if (c == splitChar && !inQuotes)
         {
            if (!string.IsNullOrEmpty(current))
               parts.Add(current);
            current = string.Empty;
            continue;
         }

         current += c;
      }

      if (!string.IsNullOrEmpty(current))
         parts.Add(current);
      return parts.ToArray();
   }
}