using System.IO;
using System.Text;
using Arcanum.API;
using Arcanum.API.Console;
using Arcanum.API.Core.IO;
using Arcanum.API.UtilServices;
using Arcanum.Core.CoreSystems.ConsoleServices.Command_Implementations.Release_Commands;

namespace Arcanum.Core.CoreSystems.ConsoleServices;

public class ConsoleServiceImpl : IConsoleService
{
   private readonly Dictionary<string, string> _macros = [];
   private readonly List<string> _history = [];

   private readonly Dictionary<string, ICommandDefinition> _commands = new();
   private int _historyIndex;
   private IOutputReceiver? _outputReceiver;

   public string Identifier { get; }
   public bool TrimQuotesOnArguments { get; set; } = true;

   private const int HISTORY_CAPACITY = 100;
   private const string MACRO_FILE = "macros.json";
   private const string HISTORY_FILE = "consoleHistory.json";
   private const string CONSOLE_DATA_FOLDER = "ConsoleData"; // contains all macros and history files
   public const string CMD_PREFIX = " > ";
   public const int CMD_PREFIX_LENGTH = 3;

   public int HistoryIndex
   {
      get => _historyIndex;
      private set => _historyIndex = Math.Clamp(value, 0, _history.Count);
   }

   bool IConsoleService.HasOutputReceiver() => _outputReceiver != null;

   public ClearanceLevel CurrentClearance { get; set; }

   private readonly IPluginHost _host;

   public ConsoleServiceImpl(IPluginHost host,
                             string identifier,
                             IOutputReceiver? outputReceiver = null,
                             DefaultCommands.CommandCategory category = DefaultCommands.CommandCategory.StandardUser)
   {
      _host = host ?? throw new ArgumentNullException(nameof(host), "Plugin host cannot be null.");
      DefaultCommands.RegisterDefaultCommands(this, category);
      ErrorLogCommands.RegisterCommands(this);
      ValidatorCommands.RegisterCommands(this);
      DataReturnCommands.RegisterCommands(this);

      Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
      _outputReceiver = outputReceiver;

      // Initialize clearance
#if DEBUG
      CurrentClearance = ClearanceLevel.Debug;
#else
      CurrentClearance = ClearanceLevel.User;
#endif

      LoadMacros();
      LoadHistory();
   }

   // The critical lists are readonly and initialized in the constructor.
   // Host and identifier are required parameters and throw if null.
   // SO we can safely assume the service is in a valid state after construction.
   public IService.ServiceState VerifyState() => IService.ServiceState.Ok;

   // --- Public API Methods ---
   public IReadOnlyList<ICommandDefinition> GetRegisteredCommandsWithoutAliases()
   {
      HashSet<ICommandDefinition> uniqueCommands = new(_commands.Values);
      return uniqueCommands.OrderBy(c => c.Name).ToList();
   }

   public void RegisterCommand(ICommandDefinition command)
   {
      ArgumentNullException.ThrowIfNull(command);

      _commands[command.Name.ToLowerInvariant()] = command;
      foreach (var alias in command.Aliases)
         _commands[alias.ToLowerInvariant()] = command;
   }

   public void UnregisterCommand(string commandName)
   {
      commandName = commandName.ToLowerInvariant();
      if (!_commands.Remove(commandName, out var commandToUnregister))
         return;

      // Remove the main name
      // Remove all aliases pointing to this command instance
      var aliasesToRemove = _commands
                           .Where(kvp => Equals(kvp.Value, commandToUnregister) &&
                                         !kvp.Key.Equals(commandToUnregister.Name,
                                                         StringComparison.InvariantCultureIgnoreCase))
                           .Select(kvp => kvp.Key)
                           .ToList();
      foreach (var alias in aliasesToRemove)
         _commands.Remove(alias);
   }

   public string[] ProcessCommand(string commandLine)
   {
      if (string.IsNullOrWhiteSpace(commandLine))
         return [];

      AddToHistory(commandLine);
      var outputLines = ExecuteCommandLogic(commandLine);

      _outputReceiver?.WriteLines([.. outputLines]);
      if (outputLines.Length == 0)
         _outputReceiver?.WriteLine("No output returned.", prefix: true);

      return outputLines;
   }

   public bool GetCommandDefinition(string commandName, out ICommandDefinition? commandDefinition)
   {
      if (_commands.TryGetValue(commandName.ToLowerInvariant(), out var cmd))
      {
         commandDefinition = cmd;
         return true;
      }

      commandDefinition = null;
      _outputReceiver?.WriteError($"Command '{commandName}' not found.");
      return false;
   }

   public bool SetAlias(string alias, string commandName)
   {
      alias = alias.ToLowerInvariant();
      commandName = commandName.ToLowerInvariant();

      if (_commands.TryGetValue(commandName, out var cmd))
      {
         // Check if alias is already a base command name
         if (_commands.Values.Any(c => c.Name.Equals(alias, StringComparison.InvariantCultureIgnoreCase) &&
                                       !Equals(c, cmd)))
         {
            _outputReceiver?.WriteError($"Cannot create alias '{alias}' as it's a name of another command.");
            return false;
         }

         _commands[alias] = cmd;
         return true;
      }

      _outputReceiver?.WriteError($"Command '{commandName}' not found to create alias '{alias}'.");
      return false;
   }

   public bool RemoveAlias(string alias)
   {
      alias = alias.ToLowerInvariant();
      // Ensure we are not removing a base command name, only an alias
      if (_commands.TryGetValue(alias, out var command) &&
          !command.Name.Equals(alias, StringComparison.InvariantCultureIgnoreCase))
      {
         _commands.Remove(alias);
         return true;
      }

      _outputReceiver?.WriteError($"Alias '{alias}' not found or it's a base command name.");
      return false;
   }

   // --- Macro Management ---

   IReadOnlyList<string> IConsoleService.GetHistory() => GetHistory();

   public bool AddMacro(string key, string value) => !string.IsNullOrWhiteSpace(key) && _macros.TryAdd(key, value);

   public bool RemoveMacro(string key) => _macros.Remove(key);

   public void RunMacro(string macroString, out string[] resultOutput)
   {
      resultOutput = _macros.TryGetValue(macroString, out var macroValue)
                        ? ExecuteCommandLogic(macroValue)
                        : [$"Macro '{macroString}' not found."];
   }

   public void ClearMacros() => _macros.Clear();
   public IReadOnlyDictionary<string, string> GetMacros() => _macros;

   // --- History Management ---
   public List<string> GetHistory() => _history; // Return a copy

   public void ClearHistory()
   {
      _history.Clear();
      _historyIndex = 0;
   }

   private void AddToHistory(string cmd) // Keep internal if the UI handles it, public if the API user should add
   {
      if (string.IsNullOrWhiteSpace(cmd))
         return;

      var historyList = _history;

      if (cmd.StartsWith(CMD_PREFIX))
         cmd = cmd[CMD_PREFIX.Length..];
      // Only add if different from the last command
      if (historyList.Count == 0 || !historyList[^1].Equals(cmd, StringComparison.OrdinalIgnoreCase))
         historyList.Add(cmd.Trim()); // Trim to remove leading/trailing spaces

      if (historyList.Count > HISTORY_CAPACITY)
         historyList.RemoveAt(0);
      _historyIndex = historyList.Count; // Point to the position after the new entry
   }

   public string? GetPreviousHistoryEntry()
   {
      if (_history.Count == 0)
         return null;

      HistoryIndex = Math.Max(0, HistoryIndex - 1);
      return _history[HistoryIndex];
   }

   public string? GetNextHistoryEntry()
   {
      if (_history.Count == 0)
         return null;

      // If at the end (after the last item), effectively means new command line
      if (HistoryIndex >= _history.Count - 1)
      {
         HistoryIndex = _history.Count; // Position for new command
         return ""; // Or null, depending on desired behavior for "next" beyond the last item
      }

      HistoryIndex = Math.Min(_history.Count - 1, HistoryIndex + 1);
      return _history[HistoryIndex];
   }

   IReadOnlyList<string> IConsoleService.GetCommandNames() => GetCommandNames();

   IReadOnlyList<string> IConsoleService.GetCommandAliases() => GetCommandAliases();

   public IReadOnlyList<ICommandDefinition> GetRegisteredCommands() => _commands.Values.Distinct().OrderBy(c => c.Name).ToList();

   // --- Static Persistence ---
   public void SaveMacros()
   {
      var jsonService = _host.GetService<IJsonProcessor>();
      var ioService = _host.GetService<IFileOperations>();

      if (jsonService == null || ioService == null)
         throw new InvalidOperationException("Required services for saving macros are not available.");

      var jsonData = jsonService.Serialize(_macros);
      ioService.WriteAllText(Path.Combine(ioService.GetArcanumDataPath,
                                          CONSOLE_DATA_FOLDER,
                                          Identifier + '_' + MACRO_FILE),
                             jsonData,
                             Encoding.UTF8);
   }

   public void LoadMacros()
   {
      var jsonService = _host.GetService<IJsonProcessor>();
      var ioService = _host.GetService<IFileOperations>();

      if (jsonService == null || ioService == null)
         throw new InvalidOperationException("Required services for loading macros are not available.");

      var filePath = Path.Combine(ioService.GetArcanumDataPath, CONSOLE_DATA_FOLDER, Identifier + '_' + MACRO_FILE);

      if (ioService.FileExists(filePath))
      {
         var jsonData = ioService.ReadAllText(filePath, Encoding.UTF8);
         if (string.IsNullOrWhiteSpace(jsonData))
         {
            _outputReceiver?.WriteError("No valid macro data file found. Please create macros first.");
            return;
         }

         if (jsonService.TryDeserialize(jsonData, out Dictionary<string, string>? loadedMacros))
         {
            _macros.Clear(); // Clear existing before loading
            foreach (var entry in loadedMacros!)
               if (!_macros.ContainsKey(entry.Key))
                  _macros[entry.Key] = entry.Value;
         }
         else
         {
            _outputReceiver?.WriteError("Failed to load macros from file. Invalid JSON format.");
         }
      }
   }

   public void SaveHistory()
   {
      var jsonService = _host.GetService<IJsonProcessor>();
      var ioService = _host.GetService<IFileOperations>();
      var filePath = Path.Combine(ioService.GetArcanumDataPath, CONSOLE_DATA_FOLDER, Identifier + '_' + HISTORY_FILE);

      if (jsonService == null || ioService == null)
         throw new InvalidOperationException("Required services for saving history are not available.");

      ioService.WriteAllTextUtf8(filePath, jsonService.Serialize(_history));
   }

   public void LoadHistory()
   {
      var jsonService = _host.GetService<IJsonProcessor>();
      var ioService = _host.GetService<IFileOperations>();
      var filePath = Path.Combine(ioService.GetArcanumDataPath, CONSOLE_DATA_FOLDER, Identifier + '_' + HISTORY_FILE);

      if (jsonService == null || ioService == null)
         throw new InvalidOperationException("Required services for loading history are not available.");

      if (ioService.FileExists(filePath))
      {
         var jsonData = ioService.ReadAllTextUtf8(filePath);
         if (string.IsNullOrWhiteSpace(jsonData))
         {
            _outputReceiver?.WriteLine("No valid history data file found. Please create history first.");
            return;
         }

         if (jsonService.TryDeserialize(jsonData, out List<string>? loadedHistory))
         {
            _history.Clear();
            if (loadedHistory != null)
               _history.AddRange(loadedHistory);
         }
         else
         {
            _outputReceiver?.WriteError("Failed to load history from file. Invalid JSON format.");
         }
      }

      _historyIndex = _history.Count; // Set the initial history index
   }

   // --- Internal Logic ---
   private string[] ExecuteCommandLogic(string input)
   {
      List<string> output = [];
      var parts = SplitStringQuotes(input, trimQuotes: TrimQuotesOnArguments); // Use property
      if (parts.Length <= 1)
         return [.. output];

      var startIndex =
         parts[0].StartsWith(CMD_PREFIX.Trim(), StringComparison.Ordinal) ? 1 : 0; // Skip CMD_PREFIX if present
      var commandName = parts[startIndex].ToLowerInvariant(); // Use ToLowerInvariant for consistency
      var args = parts.Skip(startIndex + 1).ToArray();

      if (_commands.TryGetValue(commandName, out var command))
      {
         if (CurrentClearance >= command.Clearance)
            try
            {
               output.AddRange(command.Execute(args));
            }
            catch (Exception ex)
            {
               output.Add($"❌ Error executing command '{commandName}': {ex.Message}");
               // Optionally log ex.ToString() for more details for developers
            }
         else
            output.Add($"❌ Permission denied for '{commandName}'. Requires {command.Clearance} clearance (current: {CurrentClearance}).");
      }
      else
      {
         output.Add($"❌ Unknown command: {commandName}");
         var suggestion = FindClosestCommand(commandName);
         if (!string.IsNullOrEmpty(suggestion))
            output.Add($"💡 Did you mean: {suggestion}?");
      }

      return [.. output];
   }

   // --- Utility Methods ---
   public List<string> GetCommandNames() => _commands.Values.Select(x => x.Name).Distinct().OrderBy(name => name).ToList();

   public List<string> GetCommandAliases()
   {
      HashSet<string> described = [];
      foreach (var command in _commands.Values.Distinct()) // Ensure we process each command at once
         foreach (var alias in command.Aliases)
            if (!alias.Equals(command.Name, StringComparison.InvariantCultureIgnoreCase)) // Only list actual aliases
               described.Add($"{alias} (alias for {command.Name})");
      return described.ToList().OrderBy(a => a).ToList();
   }

   private static string[] SplitStringQuotes(string cmd,
                                             char splitChar = ' ',
                                             char quoteChar = '"',
                                             bool trimQuotes = true)
   {
      List<string> parts = [];
      var inQuotes = false;
      var currentPart = new StringBuilder();

      foreach (var c in cmd)
      {
         if (c == quoteChar)
         {
            inQuotes = !inQuotes;
            if (trimQuotes)
               continue; // Skip the quote character itself if trimming
         }

         if (c == splitChar && !inQuotes)
         {
            if (currentPart.Length > 0)
               parts.Add(currentPart.ToString());
            currentPart.Clear();
            continue;
         }

         currentPart.Append(c);
      }

      if (currentPart.Length > 0)
         parts.Add(currentPart.ToString());

      return parts.ToArray();
   }

   private string? FindClosestCommand(string input)
   {
      if (string.IsNullOrWhiteSpace(input) || _commands.Count == 0)
         return null;

      // Consider only base command names for suggestions, not aliases, to avoid suggesting an alias for an alias.
      return _commands.Keys
                      .OrderBy(cmdKey => LevenshteinDistance(input, cmdKey))
                      .FirstOrDefault();
   }

   private static int LevenshteinDistance(string s1, string s2)
   {
      s1 = s1.ToLowerInvariant();
      s2 = s2.ToLowerInvariant();
      var dp = new int[s1.Length + 1, s2.Length + 1];

      for (var i = 0; i <= s1.Length; i++)
         dp[i, 0] = i;
      for (var j = 0; j <= s2.Length; j++)
         dp[0, j] = j;

      for (var i = 1; i <= s1.Length; i++)
         for (var j = 1; j <= s2.Length; j++)
         {
            var cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;
            dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                                dp[i - 1, j - 1] + cost);
         }

      return dp[s1.Length, s2.Length];
   }

   public void Clear()
   {
      _outputReceiver?.Clear();
   }

   public void SetOutputReciever(IOutputReceiver outputReceiver)
   {
      _outputReceiver = outputReceiver ??
                        throw new ArgumentNullException(nameof(outputReceiver), "Output receiver cannot be null.");
   }

   public void Unload()
   {
      SaveMacros();
      SaveHistory();
   }
}