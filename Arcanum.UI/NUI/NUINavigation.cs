using System.ComponentModel;
using System.Runtime.CompilerServices;
using Arcanum.UI.NUI.Generator;

namespace Arcanum.UI.NUI;

public class NUINavigation(int capacity)
{
   private static readonly Lazy<NUINavigation> LazyInstance = new(() => new (100));

   public static NUINavigation Instance => LazyInstance.Value;

   private readonly LinkedList<NUINavHistory> _items = [];
   private LinkedListNode<NUINavHistory>? _current;

   public void Navigate(NUINavHistory item)
   {
      if (_current != null && _current.Value.Equals(item))
         return;
      
      while (_current?.Next != null)
         _items.Remove(_current.Next);

      
      _items.AddLast(item);
      _current = _items.Last;

      if (_items.Count > capacity)
         _items.RemoveFirst();
   }

   public bool CanBack => _current?.Previous != null;
   public bool CanForward => _current?.Next != null;

   public void Back()
   {
      if (!CanBack)
         return;

      _current = _current!.Previous;
      NUIViewGenerator.GenerateAndSetView(_current!.Value);
   }

   public void Forward()
   {
      if (!CanForward)
         return;

      _current = _current!.Next;
      NUIViewGenerator.GenerateAndSetView(_current!.Value);
   }
}

public class Test : INotifyPropertyChanged
{
   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
      if (EqualityComparer<T>.Default.Equals(field, value))
         return false;

      field = value;
      OnPropertyChanged(propertyName);
      return true;
   }
}