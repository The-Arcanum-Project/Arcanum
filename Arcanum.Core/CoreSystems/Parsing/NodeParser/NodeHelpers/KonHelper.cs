using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religion.SubObjects;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class KonHelper
{
   public static bool TryGetLocation(this KeyOnlyNode node,
                                     LocationContext ctx,
                                     string source,
                                     string className,
                                     ref bool validationResult,
                                     [MaybeNullWhen(false)] out Location value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.Locations.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidLocationKey,
                                     $"Parsing {className}",
                                     key);
      validationResult = false;
      return false;
   }

   public static bool TryGetArea(this KeyOnlyNode node,
                                 LocationContext ctx,
                                 string source,
                                 string className,
                                 ref bool validationResult,
                                 [MaybeNullWhen(false)] out Area value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.Areas.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidObjectKey,
                                     $"Parsing {className}",
                                     key,
                                     nameof(Area));
      validationResult = false;
      return false;
   }

   public static bool TryGetRegion(this KeyOnlyNode node,
                                   LocationContext ctx,
                                   string source,
                                   string className,
                                   ref bool validationResult,
                                   [MaybeNullWhen(false)] out Region value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.Regions.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidObjectKey,
                                     $"Parsing {className}",
                                     key,
                                     nameof(Region));
      validationResult = false;
      return false;
   }

   public static bool TryGetProvince(this KeyOnlyNode node,
                                     LocationContext ctx,
                                     string source,
                                     string className,
                                     ref bool validationResult,
                                     [MaybeNullWhen(false)] out Province value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.Provinces.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidObjectKey,
                                     $"Parsing {className}",
                                     key,
                                     nameof(Province));
      validationResult = false;
      return false;
   }

   public static bool TryGetReligiousFaction(this KeyOnlyNode node,
                                             LocationContext ctx,
                                             string source,
                                             string className,
                                             ref bool validationResult,
                                             [MaybeNullWhen(false)] out GameObjects.Religion.ReligiousFaction value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.ReligiousFactions.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidObjectKey,
                                     $"Parsing {className}",
                                     key,
                                     nameof(GameObjects.Religion.ReligiousFaction));
      validationResult = false;
      return false;
   }

   public static bool TryGetReligiousFocus(this KeyOnlyNode node,
                                           LocationContext ctx,
                                           string source,
                                           string className,
                                           ref bool validationResult,
                                           [MaybeNullWhen(false)] out ReligiousFocus value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.ReligiousFocuses.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidObjectKey,
                                     $"Parsing {className}",
                                     key,
                                     nameof(ReligiousFocus));
      validationResult = false;
      return false;
   }

   public static bool TryParseTrait(this KeyOnlyNode node,
                                    LocationContext ctx,
                                    string actionName,
                                    string source,
                                    ref bool validationResult,
                                    [MaybeNullWhen(false)] out Trait value)
   {
      var key = node.KeyNode.GetLexeme(source);
      if (Globals.Traits.TryGetValue(key, out value))
         return true;

      ctx.SetPosition(node.KeyNode);
      DiagnosticException.LogWarning(ctx,
                                     ParsingError.Instance.InvalidObjectKey,
                                     actionName,
                                     key,
                                     nameof(Trait));
      validationResult = false;
      return false;
   }
}