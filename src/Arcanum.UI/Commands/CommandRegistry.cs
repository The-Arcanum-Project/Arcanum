using System.Diagnostics;
using System.Windows;
using Common.Logger;

namespace Arcanum.UI.Commands;

public static class CommandRegistry
{
   private static readonly Dictionary<CommandId, IAppCommand> Commands = new();
   public static IEnumerable<IAppCommand> AllCommands => Commands.Values;
   public static event Action? BindingsChanged;

   static CommandRegistry()
   {
      CommandLibrary.Initialize();
      ArcLog.Write("CRS", LogLevel.INF, "CommandRegistry initialized with {0} commands.", Commands.Count);
   }

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
      {
         ArcLog.Error("CRS", $"Command '{command.Id}' is already registered.");
         Debug.Fail("Duplicate command registration detected.");
         return;
      }

      command.Gestures.CollectionChanged += (_, _) => BindingsChanged?.Invoke();
      BindingsChanged?.Invoke();
   }

   public static IAppCommand Get(CommandId id)
   {
      if (Commands.TryGetValue(id, out var cmd))
         return cmd;

      throw new KeyNotFoundException($"Command '{id.Value}' not found. Ensure CommandLibrary.Initialize() was called.");
   }
}