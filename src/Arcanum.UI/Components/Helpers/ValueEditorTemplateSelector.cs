using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using ModValInstance = Arcanum.Core.CoreSystems.Jomini.Modifiers.ModValInstance;

namespace Arcanum.UI.Components.Helpers;

public class ValueEditorTemplateSelector : DataTemplateSelector
{
   public DataTemplate? IntegerTemplate { get; set; }
   public DataTemplate? FloatTemplate { get; set; }
   public DataTemplate? BooleanTemplate { get; set; }
   public DataTemplate DefaultTemplate { get; set; } = new();

   public override DataTemplate SelectTemplate(object? item, DependencyObject container)
   {
      if (item is not ModValInstance viewModel)
         return base.SelectTemplate(item, container) ?? DefaultTemplate;

      return viewModel.Type switch
      {
         ModifierType.Integer => IntegerTemplate ?? DefaultTemplate,
         ModifierType.Float or ModifierType.Percentage => FloatTemplate ?? DefaultTemplate,
         ModifierType.Boolean => BooleanTemplate ?? DefaultTemplate,
         _ => DefaultTemplate,
      };
   }
}