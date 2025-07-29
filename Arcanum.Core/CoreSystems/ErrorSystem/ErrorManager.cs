using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.ErrorSystem;

public static class ErrorManager
{
   public static List<Diagnostic> Diagnostics { get; } = [];
   
   public static void AddToLog(Diagnostic? diagnostic)
   {
      if (diagnostic == null)
         return;

      Diagnostics.Add(diagnostic);
   }
   
   
}