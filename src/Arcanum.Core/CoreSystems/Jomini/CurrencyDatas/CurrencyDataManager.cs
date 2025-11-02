using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.AudioTags;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;

public class CurrencyDataManager
{
   public static bool TryCreateCurrencyInstance(LocationContext ctx,
                                                Token nodeKeyNode,
                                                string source,
                                                string value,
                                                ref bool validation,
                                                [MaybeNullWhen(false)] out CurrencyData instance)
   {
      var key = nodeKeyNode.GetLexeme(source);

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
         ctx.SetPosition(nodeKeyNode);
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.InvalidCurrencyValue,
                                        $"{nameof(AudioTagsManager)}.{nameof(TryCreateCurrencyInstance)}",
                                        value);
         instance = null;
         validation = false;
         return false;
      }

      return true;
   }
}