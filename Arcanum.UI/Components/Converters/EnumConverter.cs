using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Arcanum.UI.Components.Converters;

public class EnumWithDescription(Enum value)
{
   public Enum Value { get; } = value;
   public string Description { get; } = GetDescription(value);

   private static string GetDescription(Enum value)
   {
      var field = value.GetType().GetField(value.ToString());
      var attr = field?.GetCustomAttribute<DescriptionAttribute>();
      return attr?.Description ?? null!;
   }
   
   public override bool Equals(object? obj)
   {
      return obj switch
      {
         EnumWithDescription other => other.Value.Equals(Value),
         Enum otherEnum => otherEnum.Equals(Value),
         _ => false
      };
   }

   public override int GetHashCode()
   {
      return Value.GetHashCode();
   }
   
   public override string ToString() => Value.ToString(); // Optional: affects default display
}

public class EnumValuesConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not Type { IsEnum: true } enumType)
         return null;

      var isFlags = Attribute.IsDefined(enumType, typeof(FlagsAttribute));

      var values = Enum.GetValues(enumType)
                       .Cast<Enum>()
                       .Where(v =>
                        {
                           var val = System.Convert.ToInt64(v);
                           return Enum.IsDefined(enumType, v) &&
                                  (!isFlags || (val != 0 && IsSingleBit(val)));
                        })
                       .Distinct()
                       .Select(e => new EnumWithDescription(e))
                       .ToArray();

      return values;
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
      => throw new NotImplementedException();

   private static bool IsSingleBit(long value) => (value & (value - 1)) == 0;
}

public class EnumValueConverter : IValueConverter
{
   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value == null)
         return null;

      var valueType = value.GetType();

      return !valueType.IsEnum ? null : new EnumWithDescription((Enum)value);
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if (value is not EnumWithDescription enumWithDescription)
         throw new ArgumentException("Value must be of type EnumWithDescription", nameof(value));

      return enumWithDescription.Value;
   }

}