using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Arcanum.Core.GameObjects.Court;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Religious;
using Arcanum.Core.GameObjects.Religious.SubObjects;
using Region = Arcanum.Core.GameObjects.LocationCollections.Region;

namespace Arcanum.Core.CoreSystems.Parsing.NodeParser.NodeHelpers;

public static class KonHelper
{
   public static bool TryGetLocation(this KeyOnlyNode node,
                                     ref ParsingContext pc,
                                     [MaybeNullWhen(false)] out Location value)
   {
      var key = pc.SliceString(node);
      if (Globals.Locations.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidLocationKey,
                                     key);
      return pc.Fail();
   }

   public static bool TryParseFloatValue(this KeyOnlyNode node,
                                         ref ParsingContext pc,
                                         out float value)
   {
      var lexeme = pc.SliceString(node);
      if (float.TryParse(lexeme, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.FloatParsingError,
                                     lexeme);
      value = 0f;
      return pc.Fail();
   }

   public static bool TryGetArea(this KeyOnlyNode node,
                                 ref ParsingContext pc,
                                 [MaybeNullWhen(false)] out Area value)
   {
      var key = pc.SliceString(node);
      if (Globals.Areas.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidObjectKey,
                                     key,
                                     nameof(Area));
      return pc.Fail();
   }

   public static bool TryGetRegion(this KeyOnlyNode node,
                                   ref ParsingContext pc,
                                   [MaybeNullWhen(false)] out Region value)
   {
      var key = pc.SliceString(node);
      if (Globals.Regions.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidObjectKey,
                                     key,
                                     nameof(Region));
      return pc.Fail();
   }

   public static bool TryGetProvince(this KeyOnlyNode node,
                                     ref ParsingContext pc,
                                     [MaybeNullWhen(false)] out Province value)
   {
      var key = pc.SliceString(node);
      if (Globals.Provinces.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidObjectKey,
                                     key,
                                     nameof(Province));
      return pc.Fail();
   }

   public static bool TryGetReligiousFaction(this KeyOnlyNode node,
                                             ref ParsingContext pc,
                                             [MaybeNullWhen(false)] out ReligiousFaction value)
   {
      var key = pc.SliceString(node);
      if (Globals.ReligiousFactions.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidObjectKey,
                                     key,
                                     nameof(ReligiousFaction));
      return pc.Fail();
   }

   public static bool TryGetReligiousFocus(this KeyOnlyNode node,
                                           ref ParsingContext pc,
                                           [MaybeNullWhen(false)] out ReligiousFocus value)
   {
      var key = pc.SliceString(node);
      if (Globals.ReligiousFocuses.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidObjectKey,
                                     key,
                                     nameof(ReligiousFocus));
      return pc.Fail();
   }

   public static bool TryParseTrait(this KeyOnlyNode node,
                                    ref ParsingContext pc,
                                    [MaybeNullWhen(false)] out Trait value)
   {
      var key = pc.SliceString(node);
      if (Globals.Traits.TryGetValue(key, out value))
         return true;

      pc.SetContext(node);
      DiagnosticException.LogWarning(ref pc,
                                     ParsingError.Instance.InvalidObjectKey,
                                     key,
                                     nameof(Trait));
      return pc.Fail();
   }
}