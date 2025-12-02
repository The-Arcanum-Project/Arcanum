using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.AiTags;

public class AiTagManager
{
   public static bool TryCreateAiTagInstance(ref ParsingContext pc,
                                             Token nodeKeyNode,
                                             string value,
                                             [MaybeNullWhen(false)] out AiTag instance)
   {
      using var ctx = pc.PushScope();
      var key = pc.SliceString(nodeKeyNode);

      if (ModifierManager.TryConvertValueToType(value, ModifierType.Integer, out var convertedValue))
      {
         instance = new()
         {
            UniqueId = key,
            Value = convertedValue,
            Type = ModifierType.Integer,
         };
      }
      else if (ModifierManager.TryConvertValueToType(value, ModifierType.Float, out convertedValue))
      {
         instance = new()
         {
            UniqueId = key,
            Value = convertedValue,
            Type = ModifierType.Float,
         };
      }
      else
      {
         pc.SetContext(nodeKeyNode);
         DiagnosticException.LogWarning(ref pc,
                                        ParsingError.Instance.InvalidTagTypeOrValue,
                                        value,
                                        typeof(AiTag),
                                        key);
         instance = null;
         return pc.Fail();
      }

      return true;
   }
}