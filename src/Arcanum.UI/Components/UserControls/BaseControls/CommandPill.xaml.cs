using System.Windows;
using System.Windows.Input;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class CommandPill
{
   public static readonly DependencyProperty CommandProperty =
      DependencyProperty.Register(nameof(Gesture), typeof(InputGesture), typeof(CommandPill), new(null));

   public CommandPill()
   {
      InitializeComponent();
   }

   public InputGesture Gesture
   {
      get => (InputGesture)GetValue(CommandProperty);
      set => SetValue(CommandProperty, value);
   }
}