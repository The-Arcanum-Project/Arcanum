using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class RnHelpers
{
   /// <summary>
   /// Creates a diagnostic if the RootNode is empty.
   /// </summary>
   /// <param name="rn">The root node to check</param>
   /// <param name="ctx">The <see cref="LocationContext"/> of the file.</param>
   /// <returns></returns>
   public static bool IsNodeEmptyDiagnostic(this RootNode rn, LocationContext ctx)
   {
      if (rn.Statements.Count == 0)
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.EmptyRootNode,
                                        "Parsing RootNode",
                                        ctx.FilePath);

      return rn.Statements.Count == 0;
   }

   /// <summary>
   /// Creates a diagnostic if the RootNode is empty.
   /// </summary>
   /// <param name="rn">The root node to check</param>
   /// <param name="ctx">The <see cref="LocationContext"/> of the file.</param>
   /// <param name="success">The bool the result is written to</param>
   /// <returns></returns>
   public static void IsNodeEmptyDiagnostic(this RootNode rn, LocationContext ctx, ref bool success)
   {
      success &= !IsNodeEmptyDiagnostic(rn, ctx);
   }

   /// <summary>
   /// Creates a diagnostic if the RootNode does not have exactly <paramref name="count"/> statements.
   /// </summary>
   /// <param name="rn">The root node to check</param>
   /// <param name="ctx">the <see cref="LocationContext"/> of the <see cref="RootNode"/></param>
   /// <param name="count">The expected number of <see cref="StatementNode"/>s</param>
   /// <returns></returns>
   public static bool HasXStatements(this RootNode rn, LocationContext ctx, int count)
   {
      if (rn.Statements.Count == count)
         return true;

      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidStatementCount,
                                     "Parsing RootNode",
                                     count,
                                     rn.Statements.Count);
      return false;
   }

   /// <summary>
   /// Creates a diagnostic if the RootNode does not have exactly '<paramref name="count"/>' statements.
   /// </summary>
   /// <param name="rn">The root node to check</param>
   /// <param name="ctx">the <see cref="LocationContext"/> of the <see cref="RootNode"/></param>
   /// <param name="count">The expected number of <see cref="StatementNode"/>s</param>
   /// <param name="success">The bool the result is written to</param>
   /// <returns></returns>
   public static bool HasXStatements(this RootNode rn, LocationContext ctx, int count, ref bool success)
   {
      var retVal = HasXStatements(rn, ctx, count);
      success &= retVal;
      return retVal;
   }
}