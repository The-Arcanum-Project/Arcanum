using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GlobalStates;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.Parsing.CeasarParser.ValueHelpers;

public static class LvnHelpers
{
   public static bool IsLiteralValueNode(this ValueNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         [MaybeNullWhen(false)] out LiteralValueNode value)
   {
      if (node is not LiteralValueNode lvn)
      {
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidNodeType,
                                        actionName,
                                        node.GetType().Name,
                                        nameof(LiteralValueNode),
                                        "N/A");
         value = null!;
         return false;
      }

      value = lvn;
      return true;
   }

   public static bool IsLiteralValueNode(this ValueNode node,
                                         LocationContext ctx,
                                         string actionName,
                                         ref bool validationResult,
                                         [MaybeNullWhen(false)] out LiteralValueNode value)
   {
      var returnVal = node.IsLiteralValueNode(ctx, actionName + ".ValueNode.IsLiteralValueNode", out value);
      validationResult &= returnVal;
      return returnVal;
   }

   public static bool TryParseLocationFromLvn(this LiteralValueNode lvn,
                                              LocationContext ctx,
                                              string actionName,
                                              string source,
                                              out Location location)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Locations.TryGetValue(lexeme, out location!))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        lexeme);
         location = Location.Empty;
         return false;
      }

      return true;
   }

   public static bool GetLocation(this LiteralValueNode lvn,
                                  LocationContext ctx,
                                  string actionName,
                                  string source,
                                  ref bool validationResult,
                                  [MaybeNullWhen(false)] out Location location)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Globals.Locations.TryGetValue(lexeme, out location))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.InvalidLocationKey,
                                        actionName,
                                        lexeme);
         location = Location.Empty;
         validationResult = false;
         return false;
      }

      return true;
   }

   /// <summary>
   /// Gets the enum value from the LiteralValueNode. <br/>
   /// Logs a warning if the value could not be parsed to the enum type.
   /// </summary>
   /// <param name="lvn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="enumType"></param>
   /// <param name="validationResult"></param>
   /// <param name="enumValue"></param>
   /// <returns></returns>
   public static bool GetEnum(this LiteralValueNode lvn,
                              LocationContext ctx,
                              string actionName,
                              string source,
                              Type enumType,
                              ref bool validationResult,
                              [MaybeNullWhen(false)] out object enumValue)
   {
      var lexeme = lvn.Value.GetLexeme(source);
      if (!Enum.TryParse(enumType, lexeme, true, out enumValue))
      {
         ctx.SetPosition(lvn.Value);
         DiagnosticException.LogWarning(ctx.GetInstance(),
                                        ParsingError.Instance.EnumParseError,
                                        actionName,
                                        lexeme,
                                        enumType.Name);
         enumValue = null;
         validationResult = false;
         return false;
      }

      return true;
   }

   /// <summary>
   /// Sets the enum property on the target NUI if the value could be parsed from the LiteralValueNode. <br/>
   /// Logs a warning if the value could not be parsed to the enum type.
   /// </summary>
   /// <param name="lvn"></param>
   /// <param name="ctx"></param>
   /// <param name="actionName"></param>
   /// <param name="source"></param>
   /// <param name="validationResult"></param>
   /// <param name="target"></param>
   /// <param name="nxProp"></param>
   public static void SetEnumIfValid(this LiteralValueNode lvn,
                                     LocationContext ctx,
                                     string actionName,
                                     string source,
                                     ref bool validationResult,
                                     INUI target,
                                     Enum nxProp)
   {
      if (lvn.GetEnum(ctx, actionName, source, nxProp.GetType(), ref validationResult, out var enumObj))
         Nx.ForceSet(enumObj, target, nxProp);
   }
}