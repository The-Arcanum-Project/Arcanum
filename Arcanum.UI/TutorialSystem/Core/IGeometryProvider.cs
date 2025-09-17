using System.Windows;
using System.Windows.Media;

namespace Arcanum.UI.TutorialSystem.Core;

public interface IGeometryProvider
{
    public Geometry GetGeometry(UIElement adornerElement);
}