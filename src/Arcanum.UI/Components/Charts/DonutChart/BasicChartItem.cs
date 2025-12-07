using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Arcanum.UI.Components.Charts.DonutChart;

public class BasicChartItem : IDonutChartItem
{
   private double _value;
   public string Name { get; set; } = string.Empty;
   public Brush ColorBrush { get; set; } = Brushes.Gray;

   public double Value
   {
      get => _value;
      set
      {
         _value = value;
         OnPropertyChanged();
      }
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new(name));
}