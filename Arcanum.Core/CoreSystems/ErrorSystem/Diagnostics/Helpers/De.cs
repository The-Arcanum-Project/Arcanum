using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;

public static class De
{
   /// <summary>
   /// Creates a warning log entry for an invalid content key or node type. <br/>
   /// Sets the position in the context to the provided key token. <br/>
   /// Updates the <paramref name="validation"/> flag to false.
   /// </summary>
   /// <param name="ctx"></param>
   /// <param name="key"></param>
   /// <param name="source"></param>
   /// <param name="actionName"></param>
   /// <param name="expected"></param>
   /// <param name="validation"></param>
   public static void LogInvalidContentKeyOrNode(LocationContext ctx,
                                                 Token key,
                                                 string source,
                                                 string actionName,
                                                 object expected,
                                                 ref bool validation)
   {
      ctx.SetPosition(key);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidContentKeyOrType,
                                     actionName,
                                     key.GetLexeme(source),
                                     expected);
      validation = false;
   }

   /// <summary>
   /// Creates a warning log entry for an invalid block name. <br/>
   /// Sets the position in the context to the provided key token. <br/>
   /// Updates the <paramref name="validation"/> flag to false.
   /// </summary>
   /// <param name="ctx"></param>
   /// <param name="key"></param>
   /// <param name="source"></param>
   /// <param name="actionName"></param>
   /// <param name="expected"></param>
   /// <param name="validation"></param>
   public static void LogInvalidBlockName(LocationContext ctx,
                                          Token key,
                                          string source,
                                          string actionName,
                                          object expected,
                                          ref bool validation)
   {
      ctx.SetPosition(key);
      DiagnosticException.LogWarning(ctx.GetInstance(),
                                     ParsingError.Instance.InvalidBlockName,
                                     actionName,
                                     key.GetLexeme(source),
                                     expected);
      validation = false;
   }

   public static void Warning(LocationContext ctx,
                              DiagnosticDescriptor descriptor,
                              string action,
                              params object[] args)
   {
      DiagnosticException diagnosticException = new(descriptor, args);
      diagnosticException.HandleDiagnostic(ctx, action);
   }
}