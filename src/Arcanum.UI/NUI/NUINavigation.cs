using Arcanum.Core.CoreSystems.EventDistribution;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.NUI.Generator;

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
      if (_current != null)
         if (_current.Value.Targets.Count == item.Targets.Count)
         {
            var allMatch = true;
            for (var i = 0; i < _current.Value.Targets.Count; i++)
               if (!ReferenceEquals(_current.Value.Targets[i], item.Targets[i]))
               {
                  allMatch = false;
                  break;
               }

            if (allMatch)
               return;
         }

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
      GenerateUi(_current!.Value);
   }

   public void Forward()
   {
      if (!CanForward)
         return;

      _current = _current!.Next;
      GenerateUi(_current!.Value);
   }

   public void InvalidateUi()
   {
      if (_current == null)
         return;

      GenerateUi(_current.Value);
   }

   internal static void GenerateUi(NavH navH)
   {
      if (navH.Root.Content is IDisposable oldView)
         oldView.Dispose();
      Eu5UiGen.GenerateAndSetView(navH);
   }

   public void InvalidateUi(IEu5Object target)
   {
      if (_current == null)
         return;

      // Find the first NavH in the history that contains the target
      var node = _current;
      while (true)
      {
         if (node.Value.Targets.Contains(target))
         {
            _current = node;
            InvalidateUi();
            return;
         }

         if (node.Previous == null)
            break;

         node = node.Previous;
      }
   }
}