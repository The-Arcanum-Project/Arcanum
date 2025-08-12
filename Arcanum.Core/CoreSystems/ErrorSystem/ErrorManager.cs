using System.Diagnostics;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem;

public static class ErrorManager
{
   public static List<Diagnostic> Diagnostics { get; } = [];

   public static void ClearLog()
   {
      Diagnostics.Clear();
      Debug.WriteLine("---------------------------------\nCleared diagnostics log.\n---------------------------------");
   }
   
   public static void AddToLog(Diagnostic? diagnostic)
   {
      if (diagnostic == null)
         return;

      Diagnostics.Add(diagnostic);
      Debug.WriteLine($"Added diagnostic: {diagnostic}");
   }

   public static void AddToLog(List<Diagnostic> diagnostics)
   {
      if (diagnostics.Count == 0)
         return;

      foreach (var diagnostic in diagnostics)
         Diagnostics.Add(diagnostic);
   }
}