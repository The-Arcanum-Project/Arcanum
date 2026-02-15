using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace Arcanum.UI.Commands;

public sealed class ManagedCommand : IAppCommand
{
   private readonly Predicate<object?>? _canExecute;
   private readonly Action<object> _execute;

   public ManagedCommand(CommandId id,
                         string name,
                         string description,
                         string category,
                         string scope,
                         Action<object> execute,
                         Predicate<object?>? canExecute = null)
   {
      Id = id;
      DisplayName = name;
      Description = description;
      Category = category;
      Scope = scope;
      _execute = execute;
      _canExecute = canExecute;
      Gestures = [];
      Gestures.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Tooltip));

      CommandRegistry.Register(this);
   }

   public ManagedCommand(CommandId id,
                         string name,
                         string description,
                         string category,
                         Action<object> execute,
                         Predicate<object?>? canExecute = null,
                         params string[] scopes) : this(id,
                                                        name,
                                                        description,
                                                        category,
                                                        scopes.Length > 0
                                                           ? string.Join(", ", scopes)
                                                           : throw new ArgumentNullException(nameof(scopes), "At least one scope must be provided."),
                                                        execute,
                                                        canExecute)
   {
   }

   public CommandId Id { get; }
   public string DisplayName { get; }
   public string Description { get; }
   public string Category { get; }
   public string Scope { get; }
   public ObservableCollection<InputGesture> Gestures { get; }

   public string Tooltip
   {
      get
      {
         var gestureText = Gestures.Count > 0
                              ? $"({string.Join(", ", Gestures.Select(GetGestureText))})"
                              : string.Empty;
         return $"{DisplayName} {gestureText}\n{Description}";
      }
   }

   public event EventHandler? CanExecuteChanged
   {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
   }

   // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
   public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
   public void Execute(object? parameter) => _execute(parameter!);

   public event PropertyChangedEventHandler? PropertyChanged;

   private static string GetGestureText(InputGesture gesture)
   {
      if (gesture is KeyGesture kg)
         return kg.GetDisplayStringForCulture(CultureInfo.CurrentCulture) ?? string.Empty;

      return gesture.ToString() ?? string.Empty;
   }

   private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new(name));
}