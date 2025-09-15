namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

public static class CustomItemKeyProvider
{
   public static string GetCustomItemKey(object value)
   {
      var svl = SavingUtil.GetSavingValueType(value);
      return SavingUtil.FormatValue(svl, value);
   }
}