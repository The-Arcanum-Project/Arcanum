using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class FcnHelpers
{
   public static bool GetColorDefinition(this FunctionCallNode fcn,
                                         ref ParsingContext pc,
                                         [MaybeNullWhen(false)] out JominiColor color)
   {
      using var scope = pc.PushScope();
      const string rgbFcn = "rgb";
      const string hsvFcn = "hsv";
      const string hsv360Fcn = "hsv360";

      var fcnKey = pc.SliceString(fcn);

      var args = fcn.Arguments;

      if (args.Count == 1 && fcnKey.Equals("hex", StringComparison.OrdinalIgnoreCase))
      {
         if (!args[0].IsLiteralValueNode(ref pc, out var hexNode))
            goto fail;

         var hexStr = pc.SliceString(hexNode.Value);

         // Remove '0x' or '#' prefix if present to be safe
         if (hexStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hexStr = hexStr[2..];
         else if (hexStr.StartsWith("#"))
            hexStr = hexStr[1..];

         // Parse as Hex Number
         if (!int.TryParse(hexStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexVal))
            goto fail;

         color = new JominiColor.Hex(hexVal);
         return true;

      fail:
         color = null;
         return false;
      }

      if (args.Count is < 3 or > 4)
      {
         color = null;
         return false;
      }

      switch (fcnKey)
      {
         case rgbFcn:
         case hsvFcn:
            var values = new float[4];
            // Default Alpha to 1.0 (opaque) in case only 3 args are provided
            values[3] = 1.0f;

            for (var i = 0; i < args.Count; i++)
               if (!args[i].IsLiteralValueNode(ref pc, out var node) || !NumberParsing.TryParseFloat(pc.SliceString(node.Value), ref pc, out values[i]))
                  goto fail;

            if (fcnKey == hsvFcn)
               // Pass parsed H, S, V and A
               color = new JominiColor.Hsv(values[0], values[1], values[2], values[3]);
            else // rgbFcn
            {
               // Logic to detect if inputs are 0-1 (float) or 0-255 (byte)
               // If any R, G, or B component > 1.0, assume 0-255 scale.
               var isByteScale = values[0] > 1f || values[1] > 1f || values[2] > 1f;

               byte r,
                    g,
                    b,
                    a;

               if (isByteScale)
               {
                  r = (byte)values[0];
                  g = (byte)values[1];
                  b = (byte)values[2];

                  // If there was a 4th argument, and we are in Byte Scale, 
                  // treat '128' as 128, but '0.5' might be ambiguous. 
                  // Standard convention: if other channels are bytes, alpha is usually a byte too.
                  // However, if the 4th arg was NOT present, values[3] is 1.0f.

                  if (args.Count == 3)
                     a = 255;
                  else
                     a = (byte)values[3];
               }
               else
               {
                  // 0.0 - 1.0 scale
                  r = (byte)(values[0] * 255f);
                  g = (byte)(values[1] * 255f);
                  b = (byte)(values[2] * 255f);
                  a = (byte)(values[3] * 255f);
               }

               color = new JominiColor.Rgb(r, g, b, a);
            }

            break;

         case hsv360Fcn:
            // Hsv360 usually keeps H/S/V as Ints (0-360), but Alpha is usually a Float (0-1).
            // The original code parsed them as Ints.
            var hsv360 = new int[3];
            var alpha360 = 1.0f;

            // Parse first 3 as Ints
            for (var i = 0; i < 3; i++)
               if (!args[i].IsLiteralValueNode(ref pc, out var node) ||
                   !NumberParsing.TryParseInt(pc.SliceString(node.Value), ref pc, out hsv360[i], 0, 360))
                  goto fail;

            // Parse 4th as Float if it exists
            if (args.Count == 4)
               if (!args[3].IsLiteralValueNode(ref pc, out var node) ||
                   !NumberParsing.TryParseFloat(pc.SliceString(node.Value), ref pc, out alpha360))
                  goto fail;

            color = new JominiColor.Hsv360(hsv360[0], hsv360[1], hsv360[2], alpha360);
            break;

         default:
            pc.SetContext(fcn);
            DiagnosticException.LogWarning(ref pc,
                                           ParsingError.Instance.InvalidFunctionName,
                                           fcnKey,
                                           new object[] { rgbFcn, hsvFcn, hsv360Fcn });
         fail:
            color = null;
            return pc.Fail();
      }

      return true;
   }

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
}