using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable InvertIf

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public readonly ref struct ActionScope
{
   private readonly ref int _ptr;

   private readonly string?[] _stack;
   private readonly bool _active;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ActionScope(ref int ptr, string?[] stack, string action)
   {
      _ptr = ref ptr;
      _stack = stack;

      if (_ptr < 16)
      {
         ref var stackRef = ref MemoryMarshal.GetArrayDataReference(_stack);
         Unsafe.Add(ref stackRef, _ptr) = action;
         _ptr++;
         _active = true;
      }
      else
      {
         _active = false;
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void Dispose()
   {
      if (_active)
      {
         _ptr--;
         ref var stackRef = ref MemoryMarshal.GetArrayDataReference(_stack);
         Unsafe.Add(ref stackRef, _ptr) = null!;
      }
   }
}