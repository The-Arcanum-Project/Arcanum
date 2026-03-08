using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Settings.SmallSettingsObjects;
using Arcanum.UI.Commands.KeyMap;
using Arcanum.UI.Components.Converters;
using Common.Logger;

namespace Arcanum.UI.Commands;

public static class CommandRegistry
{
   private const string NA_TEXT = "N/A";
   private static readonly Dictionary<CommandId, IAppCommand> Commands = new();
   private static readonly GestureToTextConverter GestureConverter = new();
   private static readonly InputGesture NoneGesture = new KeyGesture(Key.None);

   public static IEnumerable<IAppCommand> AllCommands => Commands.Values;
   public static bool IsInitialized { get; private set; }
   public static event Action? BindingsChanged;

   /// <summary>
   ///    Extension method to help sync a Window's InputBindings to the Registry
   /// </summary>
   public static void ApplyBindings(Window window)
   {
      window.InputBindings.Clear();
      foreach (var cmd in Commands.Values)
         foreach (var gesture in cmd.Gestures)
            window.InputBindings.Add(new(cmd, gesture));
   }

   public static void Register(IAppCommand command)
   {
      if (!Commands.TryAdd(command.Id, command))
         return;

      command.Gestures.CollectionChanged += (_, _) =>
      {
         BindingsChanged?.Invoke();
         Persist();
      };
   }

   public static IEnumerable<CommandShortcut> GetCurrentProfiles() => Commands.Values.Select(cmd => new CommandShortcut
   {
      CommandId = cmd.Id.Value,
      CommandDisplayName = cmd.DisplayName,
      Scope = cmd.Scope,
      Shortcuts = cmd.Gestures.Select(ShortcutValidator.ToChord).ToList(),
   });

   public static void ResetAllToDefault()
   {
      foreach (var cmd in Commands.Values)
         cmd.ResetToDefault();
      BindingsChanged?.Invoke();
      Persist();
   }

   private static void Persist()
   {
      if (!IsInitialized)
         return;

      var profiles = Commands.Values.Select(cmd => new CommandShortcut
      {
         CommandId = cmd.Id.Value,
         CommandDisplayName = cmd.DisplayName,
         Scope = cmd.Scope,
         Shortcuts = cmd.Gestures.Select(ShortcutValidator.ToChord).ToList(),
      });
      var map = Config.Settings.KeyMapState.GetActiveMap();
      foreach (var profile in profiles)
      {
         var existing = map.CommandProfiles.FirstOrDefault(p => p.CommandId == profile.CommandId);
         if (existing != null)
            existing.Shortcuts = [..profile.Shortcuts];
         else
            map.CommandProfiles.Add(profile);
      }

      if (IsInitialized)
         KeyMapProfile.Serialize();
   }

   public static IAppCommand Get(CommandId id)
   {
      if (Commands.TryGetValue(id, out var cmd))
         return cmd;

      ArcLog.Write("CRS", LogLevel.ERR, "Command with ID {0} not found in registry.", id.Value);
      return null!;
   }

   public static void Initialize()
   {
      CommandLibrary.Initialize();
      KeyMapProfile.Deserialize();

      var state = Config.Settings.KeyMapState.GetActiveMap();

      foreach (var profile in state.CommandProfiles)
      {
         var cmd = Get(new(profile.CommandId));

         if (cmd == null!)
            continue;

         cmd.Gestures.Clear();
         foreach (var chord in profile.Shortcuts)
            cmd.Gestures.Add(ShortcutValidator.FromChord(chord));
      }

      if (state.CommandProfiles.Count != Commands.Count)
      {
         ArcLog.Write("CRS",
                      LogLevel.WRN,
                      "Loaded {0} command profiles, but {1} commands are registered. Generating missing profiles with default shortcuts.",
                      state.CommandProfiles.Count,
                      Commands.Count);
         var existingIds = state.CommandProfiles.Select(p => p.CommandId).ToHashSet();
         var missingCommands = Commands.Values.Where(c => !existingIds.Contains(c.Id.Value));
         foreach (var cmd in missingCommands)
         {
            var newProfile = new CommandShortcut
            {
               CommandId = cmd.Id.Value,
               CommandDisplayName = cmd.DisplayName,
               Scope = cmd.Scope,
               Shortcuts = cmd.Gestures.Select(ShortcutValidator.ToChord).ToList(),
            };
            state.CommandProfiles.Add(newProfile);
         }

         KeyMapProfile.Serialize();
      }

      RunQualityAudit();

      ArcLog.Write("CRS", LogLevel.INF, "CommandRegistry initialized with {0} commands.", Commands.Count);
      IsInitialized = true;
   }

   private static void RunQualityAudit()
   {
      foreach (var cmd in Commands.Values)
      {
         if (string.IsNullOrWhiteSpace(cmd.Description) || cmd.Description == "undefined")
            ArcLog.Write("AUDIT", LogLevel.WRN, "Command '{0}' is missing a description.", cmd.Id.Value);

         if (cmd.DisplayName.Length > 30)
            ArcLog.Write("AUDIT", LogLevel.WRN, "Command '{0}' has a very long DisplayName.", cmd.Id.Value);
      }
   }

   public static InputGesture? GetGestureForCommand(CommandId id)
   {
      var cmd = Get(id);
      if (cmd == null!)
         return null!;

      return cmd.Gestures.FirstOrDefault();
   }

   public static string GetDisplayStringForGesture(InputGesture gesture)
      => GestureConverter.Convert(gesture, typeof(string), null, CultureInfo.CurrentCulture) as string ?? NA_TEXT;

   public static string GetDisplayStringForCommand(CommandId id, bool withQuotes = true)
   {
      var cmd = Get(id);
      if (cmd == null!)
         return NA_TEXT;

      var gestures = cmd.Gestures.Count > 0 ? cmd.Gestures : [NoneGesture];
      if (!withQuotes)
         return $"'{GestureToTextConverter.MultiGestureConvert(gestures)}'";

      return GestureToTextConverter.MultiGestureConvert(gestures);
   }
}