using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class FcnHelpers
{
   public static bool HasXLvnArgumentsWithTypes(this FunctionCallNode fcn,
                                                LocationContext ctx,
                                                TokenType[] type,
                                                string source,
                                                string callStack,
                                                ref bool validationResult,
                                                [MaybeNullWhen(false)] out List<LiteralValueNode> args)
   {
      if (fcn.Arguments.Count != type.Length)
      {
         ctx.SetPosition(fcn.FunctionName);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidFunctionArgumentCount,
                                        "Parsing FunctionCallNode",
                                        type.Length,
                                        fcn.Arguments.Count,
                                        fcn.FunctionName.GetLexeme(source),
                                        fcn.Arguments);
         validationResult = false;
         args = null;
         return false;
      }

      args = new(type.Length);

      for (var i = 0; i < type.Length; i++)
      {
         if (fcn.Arguments[i] is not LiteralValueNode arg)
         {
            ctx.SetPosition(fcn.Arguments[i]);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidNodeType,
                                           callStack,
                                           fcn.Arguments[i].GetType().Name,
                                           nameof(LiteralValueNode),
                                           "N/A");
            validationResult = false;
            args = null;
            return false;
         }

         if (arg.Value.Type != type[i])
         {
            ctx.SetPosition(arg.Value);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidFunctionArgumentType,
                                           callStack,
                                           type[i],
                                           arg.Value.Type,
                                           i + 1,
                                           arg.Value.GetLexeme(source),
                                           fcn.FunctionName.GetLexeme(source));
            validationResult = false;
            args = null;
            return false;
         }

         args.Add(arg);
      }

      return true;
   }

   public static bool GetColorDefinition(this FunctionCallNode fcn,
                                         LocationContext ctx,
                                         string source,
                                         string callStack,
                                         ref bool validationResult,
                                         [MaybeNullWhen(false)] out JominiColor color)
   {
      const string rgbFcn = "rgb";
      const string hsvFcn = "hsv";
      const string hsv360Fcn = "hsv360";

      var fcnKey = fcn.FunctionName.GetLexeme(source);
      if (!fcn.HasXLvnArgumentsWithTypes(ctx,
                                         [TokenType.Number, TokenType.Number, TokenType.Number],
                                         source,
                                         callStack + $".{fcnKey}",
                                         ref validationResult,
                                         out var args))
      {
         color = null;
         return false;
      }

      switch (fcnKey)
      {
         case rgbFcn:
            var rgb = new byte[3];
            for (var i = 0; i < args.Count; i++)
               if (!args[i]
                     .TryParseByte(ctx,
                                   callStack + $".{fcnKey}.ParseByte",
                                   source,
                                   ref validationResult,
                                   out rgb[i],
                                   false))
               {
                  if (!args[i]
                        .TryParseFloat(ctx,
                                       callStack + $".{fcnKey}.ParseFloat",
                                       source,
                                       ref validationResult,
                                       out var f,
                                       false))
                  {
                     if (args[i]
                        .TryParseByte(ctx,
                                      callStack + $".{fcnKey}.ParseByte",
                                      source,
                                      ref validationResult,
                                      out rgb[i]))
                        continue;
                  }
                  else
                  {
                     rgb[i] = (byte)(f * 255f);
                     continue;
                  }

                  color = null;
                  return false;
               }

            color = new JominiColor.Rgb(rgb[0], rgb[1], rgb[2]);
            break;
         case hsvFcn:
            var hsv = new float[3];
            for (var i = 0; i < args.Count; i++)
               if (NumberParsing.TryParseFloat(args[i].Value.GetLexeme(source), ctx, out hsv[i]))
               {
                  color = null;
                  return false;
               }

            color = new JominiColor.Hsv(hsv[0], hsv[1], hsv[2]);
            break;
         case hsv360Fcn:
            var hsv360 = new int[3];
            for (var i = 0; i < args.Count; i++)
               if (!NumberParsing.TryParseInt(args[i].Value.GetLexeme(source),
                                              ctx,
                                              out hsv360[i],
                                              0,
                                              360))
               {
                  color = null;
                  return false;
               }

            color = new JominiColor.Hsv360(hsv360[0], hsv360[1], hsv360[2]);
            break;
         default:
            ctx.SetPosition(fcn.FunctionName);
            DiagnosticException.LogWarning(ctx.GetInstance(),
                                           ParsingError.Instance.InvalidFunctionName,
                                           callStack,
                                           fcnKey,
                                           new object[] { rgbFcn, hsvFcn, hsv360Fcn });
            validationResult = false;
            color = null;
            return false;
      }

      return true;
   }
}