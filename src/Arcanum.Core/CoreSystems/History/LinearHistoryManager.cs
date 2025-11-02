namespace Arcanum.Core.CoreSystems.History;

/// <summary>
/// The LinearHistoryManager class provides a linear implementation of the IHistoryManager interface,
/// enabling the management of command history in a sequential manner with undo and redo functionality.
/// </summary>
public class LinearHistoryManager : IHistoryManager
{
   private readonly List<ICommand> _entries;
   private int _currentIndex = -1;

   /// <summary>
   /// Gets the settings configuration for the <see cref="LinearHistoryManager"/>. This includes
   /// parameters such as maximum history size that dictate the behavior and limitations
   /// of the command history storage.
   /// </summary>
   public LinearHistorySettings Settings { get; }

   public EventHandler<ICommand?> UndoEvent { get; } = delegate { };
   public EventHandler<ICommand?> RedoEvent { get; } = delegate { };

   public LinearHistoryManager(LinearHistorySettings settings)
   {
      Settings = settings;
      _entries = new(settings.MaxHistorySize);
   }

   public void Add(ICommand entry)
   {
      if (_currentIndex < _entries.Count - 1)
         _entries.RemoveRange(_currentIndex + 1, _entries.Count - _currentIndex - 1);

      if (_entries.Count >= Settings.MaxHistorySize)
      {
         _entries.RemoveAt(0);
         if (_currentIndex > 0)
            _currentIndex--;
      }

      _entries.Insert(++_currentIndex, entry);
   }

   public bool CanUndo => _currentIndex > 0;
   public bool CanRedo => _currentIndex < _entries.Count - 1;

   public ICommand? Undo()
   {
      if (!CanUndo)
         return null;

      UndoEvent.Invoke(this, _entries[_currentIndex]);
      return _entries[--_currentIndex];
   }

   public ICommand? Redo()
   {
      if (!CanRedo)
         return null;

      RedoEvent.Invoke(this, _entries[_currentIndex + 1]);
      return _entries[++_currentIndex];
   }

   public void Clear()
   {
      _entries.Clear();
      _currentIndex = -1;
   }

   public ICommand CurrentCommand => _entries[_currentIndex];
}