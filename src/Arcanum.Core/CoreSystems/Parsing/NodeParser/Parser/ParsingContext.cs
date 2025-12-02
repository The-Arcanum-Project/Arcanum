using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arcanum.Core.CoreSystems.Common;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

public ref struct ParsingContext
{
   private const int MAX_ACTION_STACK_SIZE = 16;
   internal int ActionStackPtr = 0;

   public ParsingContext(LocationContext context, ReadOnlySpan<char> source, string actionStack, ref bool validation)
   {
      Context = context;
      Source = source;
      Validation = ref validation;

      ActionStack = new string[MAX_ACTION_STACK_SIZE];
      ActionStack[0] = actionStack;
      ActionStackPtr = 1;
   }

   public readonly LocationContext Context;
   public readonly ReadOnlySpan<char> Source;
   internal readonly string?[] ActionStack;
   public ref bool Validation;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetContext(KeyNodeBase knb)
   {
      var loc = knb.GetLocation();
      Context.LineNumber = loc.Item1;
      Context.ColumnNumber = loc.Item2;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetContext(LiteralValueNode lvn)
   {
      var loc = lvn.GetLocation();
      Context.LineNumber = loc.Item1;
      Context.ColumnNumber = loc.Item2;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetContext(FunctionCallNode fcn)
   {
      var loc = fcn.GetLocation();
      Context.LineNumber = loc.Item1;
      Context.ColumnNumber = loc.Item2;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetContext(ValueNode vn)
   {
      var loc = vn.GetLocation();
      Context.LineNumber = loc.Item1;
      Context.ColumnNumber = loc.Item2;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetContext(StatementNode cn) => SetContext(cn.KeyNode);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public void SetContext(Token token)
   {
      Context.LineNumber = token.Line;
      Context.ColumnNumber = token.Column;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool Fail()
   {
      Validation = false;
      return false;
   }

   public string BuildStackTrace()
   {
      Span<char> buffer = stackalloc char[256];
      var pos = 0;
      for (var i = 0; i < ActionStackPtr; i++)
      {
         var action = ActionStack[i];
         if (action is null)
            continue;

         var actionSpan = action.AsSpan();
         if (pos + actionSpan.Length + 1 > buffer.Length)
            break;

         if (i > 0)
            buffer[pos++] = '.';

         actionSpan.CopyTo(buffer[pos..]);
         pos += actionSpan.Length;
      }

      return buffer[..pos].ToString();
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<char> SliceSource(int start, int length)
   {
      ref var sourceRef = ref MemoryMarshal.GetReference(Source);
      return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref sourceRef, start), length);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<char> SliceSource(StatementNode sn) => SliceSource(sn.KeyNode.Start, sn.KeyNode.Length);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public ReadOnlySpan<char> SliceSource(KeyNodeBase knb) => SliceSource(knb.Start, knb.Length);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool IsSliceEqual(StatementNode sn, string compareTo)
   {
      if (sn.KeyNode.Length != compareTo.Length)
         return false;

      return SliceSource(sn.KeyNode.Start, sn.KeyNode.Length).SequenceEqual(compareTo);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool IsSliceEqual(KeyNodeBase knb, string compareTo)
   {
      if (knb.Length != compareTo.Length)
         return false;

      return SliceSource(knb.Start, knb.Length).SequenceEqual(compareTo);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public bool IsSliceEqual(int start, int length, string compareTo)
   {
      if (length != compareTo.Length)
         return false;

      return SliceSource(start, length).SequenceEqual(compareTo);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(int start, int length) => SliceSource(start, length).ToString();

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(StatementNode sn) => SliceSource(sn.KeyNode.Start, sn.KeyNode.Length).ToString();

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(LiteralValueNode lvn) => SliceSource(lvn.Start, lvn.Length).ToString();

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(FunctionCallNode fcn) => SliceSource(fcn.FunctionName.Start, fcn.FunctionName.Length).ToString();

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(Token token) => SliceSource(token.Start, token.Length).ToString();

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(KeyNodeBase start) => SliceSource(start.Start, start.Length).ToString();

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public string SliceString(UnaryNode un) => SliceSource(un.Start, un.Length).ToString();
}

public static class ParsingContextExtensions
{
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static ActionScope PushScope(
      ref this ParsingContext context,
      [CallerMemberName] string action = "")
   {
      // We pass a reference to the INT field, and the Array object.
      // This bypasses the "ref field to ref struct" restriction.
      return new(ref context.ActionStackPtr, context.ActionStack, action);
   }
}