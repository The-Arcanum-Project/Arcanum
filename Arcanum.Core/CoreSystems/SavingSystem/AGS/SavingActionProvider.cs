using Arcanum.Core.CoreSystems.Common;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SavingActionProvider
{
   public static void ExampleCustomSavingMethod(IAgs target, PropertySavingMetadata metadata, IndentedStringBuilder sb)
   {
      object value = null!;
      Nx.ForceGet(target, metadata.NxProp, ref value);
      using (sb.Indent())
      {
         sb.AppendLine($"# Custom saving for property: {metadata.Keyword}");
         sb.AppendLine($"# Value: {value}");
         sb.AppendLine($"{metadata.Keyword} = {value}");
      }
   }

   public static void DefaultContentSaver(IAgs target, PropertySavingMetadata metadata, IndentedStringBuilder sb)
   {
      object value = null!;
      Nx.ForceGet(target, metadata.NxProp, ref value);
      sb.AppendLine($"{metadata.Keyword} {SavingUtil.GetSeparator(metadata.Separator)} {value}");
   }
}