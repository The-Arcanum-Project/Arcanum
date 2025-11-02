using Arcanum.Core.CoreSystems.Common;

namespace Arcanum.Core.CoreSystems.SavingSystem.Test;

public class Testing
{
   public static void DoSomething()
   {
      var sb = new IndentedStringBuilder();
      using (sb.Indent())
      {
      }
   }
}