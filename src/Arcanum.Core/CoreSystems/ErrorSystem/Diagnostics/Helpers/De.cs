using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics.Helpers;

public static class De
{
   /// <summary>
   /// Creates a warning log entry for an invalid content key or node type. <br/>
   /// Sets the position in the context to the provided key token.
   /// </summary>
   public static void LogInvalidContentKeyOrNode(ref ParsingContext pc,
                                                 Token key,
                                                 object expected)
   {
      pc.SetContext(key);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidContentKeyOrType,
                                     pc.SliceString(key),
                                     expected);
   }

   /// <summary>
   /// Creates a warning log entry for an invalid block name. <br/>
   /// Sets the position in the context to the provided key token. <br/>
   /// </summary>
   public static void LogInvalidBlockName(ref ParsingContext pc,
                                          Token key,
                                          object expected)
   {
      pc.SetContext(key);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidBlockName,
                                     pc.SliceString(key),
                                     expected);
      pc.Fail();
   }

   public static void Warning(ref ParsingContext pc,
                              DiagnosticDescriptor descriptor,
                              params object[] args) => Warning(pc.Context.GetInstance(), descriptor, pc.BuildStackTrace(), args);

   public static void Warning(LocationContext ctx,
                              DiagnosticDescriptor descriptor,
                              string action,
                              params object[] args)
   {
      DiagnosticException diagnosticException = new (descriptor, args);
      diagnosticException.HandleDiagnostic(ctx, action);
   }
}