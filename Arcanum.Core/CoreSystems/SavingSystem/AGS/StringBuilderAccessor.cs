using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

internal static class StringBuilderAccessor
{
   private static readonly FieldInfo? ChunkCharsField =
      typeof(StringBuilder).GetField("m_ChunkChars", BindingFlags.NonPublic | BindingFlags.Instance);

   private static readonly FieldInfo? ChunkPreviousField =
      typeof(StringBuilder).GetField("m_ChunkPrevious", BindingFlags.NonPublic | BindingFlags.Instance);

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static char[] GetChars(StringBuilder sb) => (char[])ChunkCharsField!.GetValue(sb)!;

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static StringBuilder? GetPrevious(StringBuilder sb) => (StringBuilder?)ChunkPreviousField!.GetValue(sb);
}