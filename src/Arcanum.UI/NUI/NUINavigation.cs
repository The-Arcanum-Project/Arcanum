using Arcanum.Core.CoreSystems.EventDistribution;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Generator.SpecificGenerators;

namespace Arcanum.UI.NUI;

public class NUINavigation(int capacity)
{
   private static readonly Lazy<NUINavigation> LazyInstance = new(() => new(100));

   public static NUINavigation Instance => LazyInstance.Value;

   private readonly LinkedList<NavH> _items = [];
   private LinkedListNode<NavH>? _current;

   static NUINavigation()
   {
      EventDistributor.UpdateNUI += () => Instance.InvalidateUi();
   }

   public void Navigate(NavH item)
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
      InvalidateUi();
   }

   public void Forward()
   {
      if (!CanForward)
         return;

      _current = _current!.Next;
      InvalidateUi();
   }

   public void InvalidateUi()
   {
      if (_current == null)
         return;

      MainWindowGen.GenerateAndSetView(_current!.Value);
   }
}