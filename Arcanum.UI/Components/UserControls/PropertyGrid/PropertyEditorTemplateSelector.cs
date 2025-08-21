using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.Utils.DelayedEvents;

namespace Arcanum.UI.Components.UserControls.PropertyGrid;

public class PropertyEditorTemplateSelector : DataTemplateSelector
{
   // This dictionary will be used for fast lookups.
   private Dictionary<Type, DataTemplate>? _customTemplatesMap;

   // This collection will be populated from XAML.
   public Collection<TypeTemplatePair> CustomTemplates { get; } = [];

   public DataTemplate StringTemplate { get; set; } = null!;
   public DataTemplate BoolTemplate { get; set; } = null!;
   public DataTemplate EnumTemplate { get; set; } = null!;
   public DataTemplate IntTemplate { get; set; } = null!;
   public DataTemplate DecimalTemplate { get; set; } = null!;
   public DataTemplate FloatTemplate { get; set; } = null!;
   public DataTemplate CollectionTemplate { get; set; } = null!;
   public DataTemplate ObjectTemplate { get; set; } = null!;
   public DataTemplate DefaultTemplate { get; set; } = null!;
   public DataTemplate SingleEnumTemplate { get; set; } = null!;
   public DataTemplate EnumArrayTemplate { get; set; } = null!;

   public override DataTemplate SelectTemplate(object? item, DependencyObject container)
   {
      if (item is not PropertyItem property)
         return DefaultTemplate;

      var type = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

      // Custom templates
      // Lazily create the dictionary from the collection for fast lookups.
      _customTemplatesMap ??= CustomTemplates.ToDictionary(p => p.Type, p => p.Template);

      if (_customTemplatesMap.TryGetValue(type, out var customTemplate))
         return customTemplate;

      // Check for an array of enums FIRST.
      if (type.IsArray && type.GetElementType() == typeof(Enum))
         return EnumArrayTemplate;

      // Check for a single enum.
      if (type == typeof(Enum))
         return SingleEnumTemplate;
      
      // Fallback to existing logic
      if (type == typeof(float))
         return FloatTemplate;

      if (type == typeof(string))
         return StringTemplate;

      if (type == typeof(bool))
         return BoolTemplate;

      if (type.IsEnum)
         return EnumTemplate;

      if (type == typeof(int) || type == typeof(long) || type == typeof(short))
         return IntTemplate;

      if (type == typeof(double) || type == typeof(decimal))
         return DecimalTemplate;

      if (type.IsArray ||
          type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) ||
          type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
          type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
         return CollectionTemplate;

      // This condition is now less likely to be hit for types with custom templates.
      if (type.IsClass || type.IsValueType)
         return ObjectTemplate;

      return DefaultTemplate;
   }
}