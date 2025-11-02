namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

public enum DiagnosticCategory
{
   Loading,
   Parsing,
   Rendering,
   UserInterface,
   Plugin,
   Miscellaneous, // This is used for errors that do not fit into any other category.
}

public static class DiagnosticCategoriesExtensions
{
   /// <summary>
   /// Retrieves the prefix associated with the specified <see cref="DiagnosticCategory"/> value.
   /// The prefix has at least one and at maximum three-letters that represent the diagnosticException Category.
   /// </summary>
   /// <param name="status">The diagnosticException Category for which the prefix is needed.</param>
   /// <returns>A string representing the prefix for the given diagnosticException Category.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided diagnosticException Category is not a recognized value.</exception>
   public static string GetPrefix(this DiagnosticCategory status)
   {
      return status switch
      {
         DiagnosticCategory.Loading => "LOD",
         DiagnosticCategory.Parsing => "PAR",
         DiagnosticCategory.UserInterface => "UI",
         DiagnosticCategory.Plugin => "PLG",
         DiagnosticCategory.Rendering => "REN",
         DiagnosticCategory.Miscellaneous => "MIS",
         _ => throw new ArgumentOutOfRangeException(nameof(status)),
      };
   }
}