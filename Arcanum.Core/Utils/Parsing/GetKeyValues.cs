using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.Utils.Parsing;

public static class GetKeyValues
{
   public static string[] GetKeyValuesFromContent(Content content, string[] keys, LocationContext ctx, PathObj po)
   {
      if (keys.Length == 0)
         return [];

      var keyValues = new string[keys.Length];

      ExtractKeyValueFromContent(content, keys, po, keyValues);
      
      for (var i = 0; i < keys.Length; i++)
      {
         if (!string.IsNullOrEmpty(keyValues[i]))
            continue;
         
         keyValues[i] = string.Empty;
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.MissingKeyValue,
                                        nameof(GetKeyValues).GetType().FullName!,
                                        keys[i]);
      }

      return keyValues;
   }

   private static void ExtractKeyValueFromContent(Content content, string[] keys, PathObj po, string[] keyValues)
   {
      foreach (var kvp in content.GetLineKvpEnumerator(po))
         for (var i = 0; i < keys.Length; i++)
            if (kvp.Key.Equals(keys[i]))
            {
               keyValues[i] = kvp.Value;
               break;
            }
   }

   public static string[] GetKeyValuesFromContents(List<Content> contents, string[] keys, LocationContext ctx, PathObj po)
   {
      if (keys.Length == 0)
         return [];

      var keyValues = new string[keys.Length];

      foreach (var content in contents)
         ExtractKeyValueFromContent(content, keys, po, keyValues);

      for (var i = 0; i < keys.Length; i++)
      {
         if (!string.IsNullOrEmpty(keyValues[i]))
            continue;
         
         keyValues[i] = string.Empty;
         DiagnosticException.LogWarning(ctx,
                                        ParsingError.Instance.MissingKeyValue,
                                        nameof(GetKeyValues).GetType().FullName!,
                                        keys[i]);
      }

      return keyValues;
   }
}