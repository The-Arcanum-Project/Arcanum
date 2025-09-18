using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.ModifierSystem;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.AudioTags;

public static class AudioTagsManager
{
   public static bool TryCreateModifierInstance(LocationContext ctx,
                                                Token nodeKeyNode,
                                                string source,
                                                string value,
                                                ref bool validation,
                                                [MaybeNullWhen(false)] out AudioTag instance)
   {
      var key = nodeKeyNode.GetLexeme(source);

      if (ModifierManager.TryConvertValueToType(value, ModifierType.Float, out var convertedValue))
      {
         instance = new() { UniqueId = key, Value = (float)convertedValue };
      }
      else
      {
         ctx.SetPosition(nodeKeyNode);
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.InvalidAudioTagValue,
                                        $"{nameof(AudioTagsManager)}.{nameof(TryCreateModifierInstance)}",
                                        value);
         instance = null;
         validation = false;
         return false;
      }

      return true;
   }
}