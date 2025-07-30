using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Arcanum.Core.Utils.DelayedEvents;

public sealed class PropertyItem : INotifyPropertyChanged
{
   public PropertyInfo PropertyInfo { get; init; }
   public string Category { get; init; }
   public Type Type { get; init; }

   public bool IsReadOnly { get; init; }

   private readonly Func<object> _getter;
   private readonly Action<object>? _setter;
   private object? _value;

   public event Action<PropertyItem, object?>? ValueChanged;

   public PropertyItem(PropertyInfo propertyInfo,
                       Type type,
                       Func<object> getter,
                       Action<object>? setter = null,
                       string category = "")
   {
      PropertyInfo = propertyInfo;
      Type = type;
      _getter = getter;
      _setter = setter;
      Category = category;
      IsReadOnly = setter == null;

      _value = _getter();
   }

   public string FormattedName => string.Concat(PropertyInfo.Name.Select((c, i) =>
                                                                            i > 0 && char.IsUpper(c)
                                                                               ? " " + c
                                                                               : c.ToString()));

   public object Value
   {
      get => _getter();
      set
      {
         if (Equals(_value, value))
            return;

         var oldValue = _value;
         _value = value;

         // 1. Call the original property's setter via the delegate
         _setter?.Invoke(_value!);

         // 2. Notify the UI that the 'Value' property has changed
         OnPropertyChanged();

         // 3. Raise our internal event to notify the PropertyGrid
         ValueChanged?.Invoke(this, oldValue);
      }
   }

   public string CollectionDescription
   {
      get
      {
         if (_getter() is not ICollection collection)
            return string.Empty;

         var type = collection.GetType();
         var itemType = type.IsGenericType ? type.GetGenericArguments().FirstOrDefault() : typeof(object);
         return $"{type.Name}: ({collection.Count}) Items of {itemType?.Name}";
      }
   }

   public static PropertyItem FromExpression<TModel, TProp>(TModel instance, Expression<Func<TModel, TProp>> expr)
   {
      var member = (MemberExpression)expr.Body;
      var propInfo = (PropertyInfo)member.Member;

      return new(propertyInfo: propInfo,
                 type: typeof(TProp),
                 getter: () => propInfo.GetValue(instance)!,
                 setter: v => propInfo.SetValue(instance, Convert.ChangeType(v, propInfo.PropertyType)));
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}