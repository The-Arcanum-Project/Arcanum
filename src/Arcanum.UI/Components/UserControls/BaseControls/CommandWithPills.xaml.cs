using System.Windows;
using Arcanum.UI.Commands;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class CommandWithPills
{
   public static readonly DependencyProperty AppCommandProperty =
      DependencyProperty.Register(nameof(AppCommand), typeof(IAppCommand), typeof(CommandWithPills), new(null));

   public CommandWithPills()
   {
      InitializeComponent();
   }

   public IAppCommand AppCommand
   {
      get => (IAppCommand)GetValue(AppCommandProperty);
      set => SetValue(AppCommandProperty, value);
   }
}