using System.Diagnostics.CodeAnalysis;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.CoreSystems.Parsing.NodeParser.Parser;

namespace Arcanum.Core.CoreSystems.Jomini.CurrencyDatas;

public class CurrencyDataManager
{
   public static bool TryCreateCurrencyInstance(ref ParsingContext pc,
                                                string value,
                                                Token nodeKeyNode,
                                                [MaybeNullWhen(false)] out CurrencyData instance)
   {
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
                                        ParsingError.Instance.InvalidCurrencyValue,
                                        value);
         instance = null;
         pc.Fail();
         return false;
      }

      return true;
   }
}