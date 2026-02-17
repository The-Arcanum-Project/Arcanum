using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.KeyMap;

namespace Arcanum.UI.Components.UserControls.Settings;

public class ShortcutRecorderViewModel : INotifyPropertyChanged
{
   public required string CommandName { get; set; } = "undefined";
   public required string CommandScope { get; set; } = "undefined";

   public ShortcutStroke? FirstStroke
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
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

   public event PropertyChangedEventHandler? PropertyChanged;

   private void UpdateConflicts()
   {
      Conflicts.Clear();
      if (FirstStroke == null)
         return;

      var currentChord = new ShortcutChord(FirstStroke, IsSecondStrokeEnabled ? SecondStroke : null);

      // Call your CORE logic
      // var results = ShortcutValidator.FindConflicts(currentChord,
      //                                                   CommandScope,
      //                                                   CommandRegistry.GetAllProfiles(), // Registry helper to get DTOs
      //                                                   currentCommandId // Passed in constructor
      //                                                  );
      //
      // foreach (var r in results)
      //    Conflicts.Add(r);
   }

   protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}