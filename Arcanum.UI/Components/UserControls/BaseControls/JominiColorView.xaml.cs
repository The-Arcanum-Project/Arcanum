using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public partial class JominiColorView
{
   public JominiColorView(JominiColor color)
   {
      InitializeComponent();

      ColorBorder.Background = new SolidColorBrush(color.ToMediaColor());
      ColorTextBlock.Text = color.ToString();
   }
}