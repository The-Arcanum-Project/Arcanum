using System.Windows;

namespace Arcanum.UI.Components.Windows.PopUp;

public partial class PropertyGridWindow
{
   public PropertyGridWindow(object selectedObject)
   {
      SelectedObject = selectedObject;
      InitializeComponent();
   }

   public static readonly DependencyProperty SelectedObjectProperty =
      DependencyProperty.Register(nameof(SelectedObject),
                                  typeof(object),
                                  typeof(PropertyGridWindow),
                                  new FrameworkPropertyMetadata(null,
                                                                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

   public object SelectedObject
   {
      get => GetValue(SelectedObjectProperty);
      set => SetValue(SelectedObjectProperty, value);
   }
}