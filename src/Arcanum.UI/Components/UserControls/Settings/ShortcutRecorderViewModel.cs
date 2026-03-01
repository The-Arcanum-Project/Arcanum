using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Arcanum.Core.CoreSystems.KeyMap;
using Arcanum.UI.Commands;
using Arcanum.UI.Commands.KeyMap;

namespace Arcanum.UI.Components.UserControls.Settings;

public class ShortcutRecorderViewModel : INotifyPropertyChanged
{
   public required string CommandId { get; init; }
   public required string CommandName { get; set; } = "undefined";
   public required string CommandDescription { get; set; } = "undefined";
   public required string CommandScope { get; set; } = "undefined";

   public ShortcutStroke? FirstStroke
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Validate();
         UpdateConflicts();
      }
   }

   public ShortcutStroke? SecondStroke
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Validate();
         UpdateConflicts();
      }
   }

   public bool IsSecondStrokeEnabled
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         Validate();
         UpdateConflicts();
      }
   }

   public int ActiveSlotIndex
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   }

   public ObservableCollection<ConflictResult> Conflicts { get; } = new();

   public string ValidationError
   {
      get;
      set
      {
         var oldError = field;
         field = value;

         if (oldError != field)
         {
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasValidationError));
            OnPropertyChanged(nameof(IsSelectionValid));
         }
      }
   } = "";

   public bool HasValidationError => !string.IsNullOrEmpty(ValidationError);

   public bool IsSelectionValid => FirstStroke != null && !HasValidationError;

   public event PropertyChangedEventHandler? PropertyChanged;

   private void Validate()
   {
      ValidationError = "";

      if (FirstStroke != null && !IsValidWpfStroke(FirstStroke))
         ValidationError = $"Modifier key required for '{FirstStroke.Key}'";

      else if (IsSecondStrokeEnabled && SecondStroke != null && !IsValidWpfStroke(SecondStroke))
         ValidationError = $"Modifier key required for '{SecondStroke.Key}'";

      OnPropertyChanged(nameof(IsSelectionValid));
   }

   private static bool IsValidWpfStroke(ShortcutStroke stroke)
   {
      if (!Enum.TryParse<Key>(stroke.Key, out var key))
         return false;

      var mods = Enum.Parse<ModifierKeys>(stroke.Modifiers);
      if (mods != ModifierKeys.None)
         return true;

      switch (key)
      {
         // WPF KeyGesture allows these solo:
         case >= Key.F1 and <= Key.F24:
         case Key.Insert or Key.Delete or Key.Back or Key.Enter or Key.Tab or Key.Escape:
         case >= Key.NumPad0 and <= Key.Divide:
            return true;
      }

      return false;
   }

   private void UpdateConflicts()
   {
      Conflicts.Clear();
      if (FirstStroke == null)
         return;

      var currentChord = new ShortcutChord(FirstStroke, IsSecondStrokeEnabled ? SecondStroke : null);

      // Query the Registry for the current state of all other commands
      var allProfiles = CommandRegistry.GetCurrentProfiles();

      var results = ShortcutValidator.FindConflicts(currentChord,
                                                    CommandScope,
                                                    allProfiles,
                                                    CommandId // You should pass the current Command's ID string to the VM
                                                   );

      foreach (var r in results)
         Conflicts.Add(r);
   }

   protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}