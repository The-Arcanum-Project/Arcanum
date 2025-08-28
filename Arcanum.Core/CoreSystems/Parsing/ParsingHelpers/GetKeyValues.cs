using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.Parsing.ParsingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ParsingHelpers;

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

   /// <summary>
   /// Finds the values for the specified keys in the provided list of content elements. <br/>
   /// Values can be marked as optional by prefixing the key with a '?' character.<br/>
   /// If an optional key is not found, its value will be set to string.Empty.<br/>
   /// If a required key is not found, a warning will be logged.
   /// </summary>
   /// <param name="contents"></param>
   /// <param name="keys"></param>
   /// <param name="ctx"></param>
   /// <param name="po"></param>
   /// <returns></returns>
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
         // This value is optional
         if (keys[i].StartsWith('?'))
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