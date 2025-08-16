using System.ComponentModel;
using System.Reflection;

namespace Arcanum.Core.Utils.PropertyHelpers;

public static class DefaultPropHelper
{
   public static void SetAllPropsToDefault<T>(this T obj) where T : class
   {
      foreach (var prop in obj.GetType().GetProperties())
         prop.SetPropertyToDefault(obj);
   }

   public static void SetPropertyToDefault<T>(this T obj, string propName) where T : class
   {
      if (obj == null!)
         return;

      obj.GetType().GetProperty(propName)?.SetPropertyToDefault(obj);
   }

   public static void SetPropertyToDefault<T>(this PropertyInfo prop, T obj) where T : class
   {
      if (!prop.CanWrite || !prop.PropertyType.IsValueType)
         return;

      if (prop.GetCustomAttributes(typeof(DefaultValueAttribute), false) is DefaultValueAttribute[]
          {
             Length: > 0,
          } attrs)
         prop.SetValue(obj, attrs[0].Value);
   }
}