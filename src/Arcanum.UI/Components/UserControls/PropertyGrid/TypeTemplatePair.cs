using System.Windows;
using System.Windows.Markup;

namespace Arcanum.UI.Components.UserControls.PropertyGrid;

// This allows you to set the Type property in XAML like: Type="{x:Type local:MyClass}"
[ContentProperty(nameof(Template))]
public class TypeTemplatePair
{
   // The Type that this template is for
   public Type Type { get; set; } = null!;

   // The DataTemplate to use for this type
   public DataTemplate Template { get; set; } = null!;
}