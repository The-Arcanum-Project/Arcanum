using System.Windows.Media;
using Arcanum.UI.NUI.Generator;

namespace Arcanum.UI.Components.Windows.HelpWindow.ViewModels;

public record LocationGridCell(bool IsPrimary, bool IsSecondary)
{
   public Brush Background => IsPrimary
                                 ? ControlFactory.BlueBrush
                                 : IsSecondary
                                    ? ControlFactory.BlueSelectionBackBrush
                                    : ControlFactory.DisabledForegroundBrush;
}