using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class FcnHelpers
{
   extension(FunctionCallNode fcn)
   {
      public bool HasXLvnArgumentsWithTypes(ref ParsingContext pc,
                                            TokenType[] type,
                                            [MaybeNullWhen(false)] out List<LiteralValueNode> args)
      {
         using var scope = pc.PushScope();
         if (fcn.Arguments.Count != type.Length)
         {
            pc.SetContext(fcn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFunctionArgumentCount,
                                           type.Length,
                                           fcn.Arguments.Count,
                                           pc.SliceString(fcn.FunctionName),
                                           fcn.Arguments);
            args = null;
            return pc.Fail();
         }

         args = new(type.Length);

         for (var i = 0; i < type.Length; i++)
         {
            if (fcn.Arguments[i] is not LiteralValueNode arg)
            {
               pc.SetContext(fcn.Arguments[i]);
               DiagnosticException.LogWarning(ref pc,
                                              ParsingError.Instance.InvalidNodeType,
                                              fcn.Arguments[i].GetType().Name,
                                              nameof(LiteralValueNode),
                                              "N/A");
               args = null;
               return pc.Fail();
            }

            if (arg.Value.Type != type[i])
            {
               pc.SetContext(arg);
               DiagnosticException.LogWarning(ref pc,
                                              ParsingError.Instance.InvalidFunctionArgumentType,
                                              type[i],
                                              arg.Value.Type,
                                              i + 1,
                                              pc.SliceString(arg),
                                              pc.SliceString(fcn.FunctionName));
               args = null;
               return pc.Fail();
            }

            args.Add(arg);
         }

         return true;
      }
   }

   public static bool GetColorDefinition(this FunctionCallNode fcn,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out JominiColor color)
   {
      using var scope = pc.PushScope();
      const string rgbFcn = "rgb";
      const string hsvFcn = "hsv";
      const string hsv360Fcn = "hsv360";

      var fcnKey = pc.SliceString(fcn);
      if (!fcn.HasXLvnArgumentsWithTypes(ref pc,
                                         [TokenType.Number, TokenType.Number, TokenType.Number],
                                         out var args))
      {
         color = null;
         return false;
      }

      switch (fcnKey)
      {
         // var rgb = new byte[3];
         // for (var i = 0; i < args.Count; i++)
         //    if (!args[i]
         //          .TryParseByte(ref pc,
         //                        out rgb[i],
         //                        false))
         //    {
         //       if (!args[i]
         //             .TryParseFloat(ref pc,
         //                            out var f,
         //                            false))
         //       {
         //          if (args[i]
         //             .TryParseByte(ref pc,
         //                           out rgb[i]))
         //             continue;
         //       }
         //       else
         //       {
         //          rgb[i] = (byte)(f * 255f);
         //          continue;
         //       }
         //
         //       color = null;
         //       return false;
         //    }
         //
         // color = new JominiColor.Rgb(rgb[0], rgb[1], rgb[2]);
         // break;
         case rgbFcn:
         case hsvFcn:
            var hsv = new float[3];
            for (var i = 0; i < args.Count; i++)
               if (!NumberParsing.TryParseFloat(pc.SliceString(args[i].Value), ref pc, out hsv[i]))
               {
                  color = null;
                  return false;
               }

            color = new JominiColor.Hsv(hsv[0], hsv[1], hsv[2]);
            if (fcnKey == rgbFcn)
            {
               if (hsv[0] > 1f || hsv[1] > 1f || hsv[2] > 1f)
                  color = new JominiColor.Rgb((byte)hsv[0], (byte)hsv[1], (byte)hsv[2]);
               else
                  color = new JominiColor.Rgb((byte)(hsv[0] * 255f), (byte)(hsv[1] * 255f), (byte)(hsv[2] * 255f));
            }

            break;
         case hsv360Fcn:
            var hsv360 = new int[3];
            for (var i = 0; i < args.Count; i++)
               if (!NumberParsing.TryParseInt(pc.SliceString(args[i].Value),
                                              ref pc,
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
            pc.SetContext(fcn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFunctionName,
                                           fcnKey,
                                           new object[] { rgbFcn, hsvFcn, hsv360Fcn });
            color = null;
            return pc.Fail();
      }

      return true;
   }
}