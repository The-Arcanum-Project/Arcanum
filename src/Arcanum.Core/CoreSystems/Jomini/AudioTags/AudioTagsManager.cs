using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.AudioTags;

public static class AudioTagsManager
{
   public static bool TryCreateModifierInstance(ref ParsingContext pc,
                                                Token nodeKeyNode,
                                                string value,
                                                [MaybeNullWhen(false)] out AudioTag instance)
   {
      using var ctx = pc.PushScope();
      var key = pc.SliceString(nodeKeyNode);

      if (ModifierManager.TryConvertValueToType(value, ModifierType.Float, out var convertedValue))
      {
         instance = new() { UniqueId = key, Value = convertedValue };
      }
      else
      {
         pc.SetContext(nodeKeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidTagTypeOrValue,
                                        value,
                                        typeof(AudioTag),
                                        key);
         instance = null;
         return pc.Fail();
      }

      return true;
   }
}