using Arcanum.Core.CoreSystems.Common;
using Nexus.Core;

namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class SavingActionProvider
{
   public static void ExampleCustomSavingMethod(IAgs target, SavingMetaData metaData, IndentedStringBuilder sb)
   {
      object value = null!;
      Nx.ForceGet(target, metaData.NxProp, ref value);
      using (sb.Indent())
      {
         sb.AppendLine($"# Custom saving for property: {metaData.Keyword}");
         sb.AppendLine($"# Value: {value}");
         sb.AppendLine($"{metaData.Keyword} = {value}");
      }
   }

   public static void DefaultContentSaver(IAgs target, SavingMetaData metaData, IndentedStringBuilder sb)
   {
      object value = null!;
      Nx.ForceGet(target, metaData.NxProp, ref value);
      sb.AppendLine($"{metaData.Keyword} {SavingUtil.GetSeparator(metaData.Separator)} {value}");
   }
}