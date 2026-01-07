using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;
using Area = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Area;
using Location = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Location;
using Province = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Province;
using Region = Arcanum.Core.GameObjects.InGame.Map.LocationCollections.Region;
using ReligiousFaction = Arcanum.Core.GameObjects.InGame.Religious.ReligiousFaction;
using ReligiousFocus = Arcanum.Core.GameObjects.InGame.Religious.SubObjects.ReligiousFocus;
using Trait = Arcanum.Core.GameObjects.InGame.Court.Trait;

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
      if (float.TryParse(lexeme,
                         NumberStyles.Float | NumberStyles.AllowLeadingSign,
                         CultureInfo.InvariantCulture,
                         out value))
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