#region

using System.Collections;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Arcanum.Core.GameObjects.BaseTypes;

#endregion

namespace Arcanum.UI.NUI.Generator.StructConverters;

public class Eu5ShortInfoConverter : IMultiValueConverter
{
   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      // The first value is our 'primary' object
      // The rest are the values of the properties we are watching
      if (values.Length < 1 || values[0] is not IEu5Object primary)
         return string.Empty;

      var sb = new StringBuilder();
      var fields = primary.NUISettings.ShortInfoFields;

      // Start at 1 because 0 is the 'primary' object itself
      for (var i = 0; i < fields.Length; i++)
      {
         var nxProp = fields[i];
         // Get the value from the MultiBinding's provided values array
         var value = i + 1 < values.Length ? values[i + 1] : null;

         var itemType = primary.GetNxItemType(nxProp);

         if (itemType == null)
            sb.Append(value);
         else if (value is IEnumerable enumerable and not string)
         {
            var count = enumerable.Cast<object>().Count();
            if (count == 0)
               continue;

            sb.Append(nxProp).Append(": ").Append(count);
         }

         sb.Append("; ");
      }

      return sb.ToString().TrimEnd(' ', ';');
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}