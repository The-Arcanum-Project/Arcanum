using System.ComponentModel;
using System.Windows.Media;

namespace Arcanum.UI.Components.Charts.DonutChart;

public interface IDonutChartItem : INotifyPropertyChanged
{
   string Name { get; }
   double Value { get; }
   Brush ColorBrush { get; }
}