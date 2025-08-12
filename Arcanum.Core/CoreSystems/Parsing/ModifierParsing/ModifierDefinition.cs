using System.Diagnostics;
using Arcanum.Core.CoreSystems.SavingSystem;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.Parsing.ModifierParsing;

public enum IsModifierGood
{
   Good,
   Bad,
   Neutral,
}

[Flags]
public enum BiasType
{
   None,
   Opinion,
   Trust,
   Voting,
}

public class ModifierDefinition
{
   public string Name { get; set; }
   public IsModifierGood IsGood { get; set; } = IsModifierGood.Good;
   public int NumDecimals { get; set; } = 2;
   public int Min { get; set; }
   public string Category { get; set; } = string.Empty;
   public bool Ai { get; set; }
   public bool IsBoolean { get; set; }
   public bool IsPercentage { get; set; }
   public bool ShouldShowInModifierTab { get; set; } = true;
   public bool IsAlreadyPercent { get; set; }
   public bool ScaleWithPop { get; set; }
   public string Format { get; set; } = string.Empty;
   public BiasType Bias { get; set; } = BiasType.None;

   public ModifierDefinition(string name)
   {
      Name = name;
   }

   public override string ToString()
   {
      return $"{Name} \t(Good: {IsGood})\n\tIsBoolean: {IsBoolean}, IsPercentage: {IsPercentage}, " +
             $"ShouldShowInModifierTab: {ShouldShowInModifierTab}, Ai: {Ai}, ScaleWithPop: {ScaleWithPop}," +
             $"IsAlreadyPercent: {IsAlreadyPercent}\n\tNumDecimals: {NumDecimals}, Min: {Min}" +
             $", Format: {Format}, Category: {Category}, Bias: {Bias}";
   }

   // Name,IsGood,Category,IsBoolean,IsPercentage,ShouldShowInModifierTab,NumDecimals,Ai,Min,IsAlreadyPercent,Format,BiasType,ScaleWithPop
   public string ToCsv()
   {
      return $"{Name},{IsGood.ToString().ToLower()},{Category},{(IsBoolean ? "yes" : "no")}," +
             $"{(IsPercentage ? "yes" : "no")},{(ShouldShowInModifierTab ? "yes" : "no")}," +
             $"{NumDecimals},{(Ai ? "yes" : "no")},{Min}," +
             $"{(IsAlreadyPercent ? "yes" : "no")},{Format},{Bias.ToString().ToLower()},{(ScaleWithPop ? "yes" : "no")}";
   }
   public static string GetCsvHeader()
   {
      return "Name,IsGood,Category,IsBoolean,IsPercentage,ShouldShowInModifierTab,NumDecimals,Ai,Min,IsAlreadyPercent,Format,BiasType,ScaleWithPop";
   }
}

public static class ParseModifiers
{
   public static List<ModifierDefinition> Load()
   {
      var filePath = FileManager.GetDependentPath("game", "main_menu", "common", "modifier_types", "00_modifier_types.txt");
      var (blocks, elements) = ParsingSystem.ElementParser.GetElements(PathObj.Empty, IO.IO.ReadAllTextUtf8(filePath)!);

      if (elements.Count != 0)
         Debug.WriteLine($"[ParseModifiers] Loaded {elements.Count} elements but expected 0.");

      List<ModifierDefinition> definitions = new(blocks.Count);
      foreach (var block in blocks)
      {
         var mod = new ModifierDefinition(block.Name);
         if (block.ContentElements.Count > 1)
            Debug.WriteLine($"[ParseModifiers] Modifier {block.Name} has {block.ContentElements.Count} content elements, expected 1.");
         if (block.ContentElements.Count == 0)
         {
            definitions.Add(mod);
            continue;
         }

         foreach (var kvp in block.ContentElements[0].GetLineKvpEnumerator(PathObj.Empty))
            switch (kvp.Key)
            {
               case "is_good":
                  mod.IsGood = kvp.Value switch
                  {
                     "good" => IsModifierGood.Good,
                     "bad" => IsModifierGood.Bad,
                     "neutral" => IsModifierGood.Neutral,
                     _ => throw new ArgumentException($"Invalid value for is_good: {kvp.Value}"),
                  };
                  break;
               case "category":
                  mod.Category = kvp.Value;
                  break;
               case "is_bool":
                  mod.IsBoolean = kvp.Value switch
                  {
                     "yes" => true,
                     "no" => false,
                     _ => throw new ArgumentException($"Invalid value for is_bool: {kvp.Value}"),
                  };
                  break;
               case "is_percent":
                  mod.IsPercentage = kvp.Value switch
                  {
                     "yes" => true,
                     "no" => false,
                     _ => throw new ArgumentException($"Invalid value for is_percentage: {kvp.Value}"),
                  };
                  break;
               case "should_show_in_modifiers_tab":
                  mod.ShouldShowInModifierTab = kvp.Value switch
                  {
                     "yes" => true,
                     "no" => false,
                     _ => throw new ArgumentException($"Invalid value for should_show_in_modifiers_tab: {kvp.Value}"),
                  };
                  break;
               case "num_decimals":
                  if (int.TryParse(kvp.Value, out var numDecimals))
                     mod.NumDecimals = numDecimals;
                  else
                     Debug.WriteLine($"[ParseModifiers] Invalid num_decimals value for modifier {mod.Name}: {kvp.Value}");
                  break;
               case "ai":
                  mod.Ai = kvp.Value switch
                  {
                     "yes" => true,
                     "no" => false,
                     _ => throw new ArgumentException($"Invalid value for ai: {kvp.Value}"),
                  };
                  break;
               case "min":
                  if (int.TryParse(kvp.Value, out var min))
                     mod.Min = min;
                  else
                     Debug.WriteLine($"[ParseModifiers] Invalid min value for modifier {mod.Name}: {kvp.Value}");
                  break;
               case "already_percent":
                  mod.IsAlreadyPercent = kvp.Value switch
                  {
                     "yes" => true,
                     "no" => false,
                     _ => throw new ArgumentException($"Invalid value for already_percent: {kvp.Value}"),
                  };
                  break;
               case "format":
                  mod.Format = kvp.Value;
                  break;
               case "bias_type":
                  mod.Bias |= kvp.Value switch
                  {
                     "none" => BiasType.None,
                     "opinion" => BiasType.Opinion,
                     "trust" => BiasType.Trust,
                     "voting" => BiasType.Voting,
                     _ => throw new ArgumentException($"Invalid value for bias_type: {kvp.Value}"),
                  };
                  break;
               case "scale_with_pop":
                  mod.ScaleWithPop = kvp.Value switch
                  {
                     "yes" => true,
                     "no" => false,
                     _ => throw new ArgumentException($"Invalid value for scale_with_pop: {kvp.Value}"),
                  };
                  break;

               default:
                  Debug.WriteLine($"[ParseModifiers] Unknown key '{kvp.Key}' in modifier {mod.Name} with value '{kvp.Value}'");
                  break;
            }

         definitions.Add(mod);
      }

      Debug.WriteLine($"[ParseModifiers] Loaded {definitions.Count} modifier definitions from {filePath}.");
      return definitions;
   }
}