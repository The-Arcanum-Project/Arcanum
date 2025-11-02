namespace Arcanum.Core.Utils;

public static class FlagsEnumExtensions
{
   /// <summary>
   /// Gets a list of strings for the individual ATOMIC (power of two) flags that are set.
   /// This non-generic version is suitable for when the enum type is not known at compile time.
   /// </summary>
   public static List<string> GetSetAtomicFlagNames(this Enum enumValue)
   {
      var setFlags = new List<string>();
      var enumType = enumValue.GetType();

      if (!enumType.IsDefined(typeof(FlagsAttribute), false))
         return [enumValue.ToString()];

      var enumValueAsLong = Convert.ToInt64(enumValue);

      foreach (var flag in Enum.GetValues(enumType))
      {
         var flagAsLong = Convert.ToInt64(flag);

         // Skip the 'None = 0' value
         if (flagAsLong == 0)
            continue;

         // Check if the flag is a power of two (an atomic flag)
         var isAtomicFlag = (flagAsLong & (flagAsLong - 1)) == 0;
         if (isAtomicFlag && (enumValueAsLong & flagAsLong) == flagAsLong)
            setFlags.Add(flag.ToString()!);
      }

      // Handle the case where the value is exactly 0 and a "None" or 0-value name exists.
      if (setFlags.Count == 0 && enumValueAsLong == 0)
      {
         var zeroFlagName = Enum.GetName(enumType, 0);
         if (zeroFlagName != null)
            return [zeroFlagName];
      }

      return setFlags;
   }
}