namespace Arcanum.Core.CoreSystems.SavingSystem.AGS;

/// <summary>
/// The class containing all logic to provide a custom key for a given item in the saving process.
/// </summary>
public static class CustomItemKeyProvider
{
   public static string GetCustomItemKey(object value)
   {
      var svl = SavingUtil.GetSavingValueType(value);
      return SavingUtil.FormatValue(svl, value);
   }
}