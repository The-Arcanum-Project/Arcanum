using System.Windows;
using System.Windows.Data;

namespace Arcanum.UI.Components.UserControls.BaseControls.AutoCompleteBox;

sealed class DependencyVariable<T>
   : DependencyObject
{
   public static DependencyProperty ValueProperty { get; } = DependencyProperty.Register(nameof(Value),
       typeof(T),
       typeof(DependencyVariable<T>));

   public T Value
   {
      get => (T)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
   }

   public void SetBinding(Binding binding)
   {
      BindingOperations.SetBinding(this, ValueProperty, binding);
   }

   public void SetBinding(object dataContext, string propertyPath)
   {
      SetBinding(new(propertyPath) { Source = dataContext });
   }
}