using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Arcanum.UI.Components.Charts.DonutChart;

public partial class DonutChart : UserControl
{
   public DonutChart() => InitializeComponent();

   public static readonly DependencyProperty ShowLegendValuesProperty =
      DependencyProperty.Register(nameof(ShowLegendValues), typeof(bool), typeof(DonutChart), new(true));

   public static readonly DependencyProperty ShowLegendPercentageProperty =
      DependencyProperty.Register(nameof(ShowLegendPercentage), typeof(bool), typeof(DonutChart), new(false));

   private static readonly DependencyPropertyKey CurrentTotalPropertyKey =
      DependencyProperty.RegisterReadOnly(nameof(CurrentTotal), typeof(double), typeof(DonutChart), new(0.0));

   public static readonly DependencyProperty CurrentTotalProperty = CurrentTotalPropertyKey.DependencyProperty;

   public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DonutChart), new(null, OnDataChanged));

   public static readonly DependencyProperty CenterTextProperty =
      DependencyProperty.Register(nameof(CenterText), typeof(string), typeof(DonutChart), new(string.Empty));

   public static readonly DependencyProperty ChartSizeProperty =
      DependencyProperty.Register(nameof(ChartSize), typeof(double), typeof(DonutChart), new(160.0, OnVisualChanged));

   public static readonly DependencyProperty StrokeThicknessProperty =
      DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(DonutChart), new(20.0, OnVisualChanged));

   public bool ShowLegendValues
   {
      get => (bool)GetValue(ShowLegendValuesProperty);
      set => SetValue(ShowLegendValuesProperty, value);
   }

   public bool ShowLegendPercentage
   {
      get => (bool)GetValue(ShowLegendPercentageProperty);
      set => SetValue(ShowLegendPercentageProperty, value);
   }

   public double CurrentTotal
   {
      get => (double)GetValue(CurrentTotalProperty);
      private set => SetValue(CurrentTotalPropertyKey, value);
   }

   public IEnumerable ItemsSource
   {
      get => (IEnumerable)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
   }

   public string CenterText
   {
      get => (string)GetValue(CenterTextProperty);
      set => SetValue(CenterTextProperty, value);
   }

   public double ChartSize
   {
      get => (double)GetValue(ChartSizeProperty);
      set => SetValue(ChartSizeProperty, value);
   }

   public double StrokeThickness
   {
      get => (double)GetValue(StrokeThicknessProperty);
      set => SetValue(StrokeThicknessProperty, value);
   }

   private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var chart = (DonutChart)d;
      if (e.OldValue is INotifyCollectionChanged oldC)
         oldC.CollectionChanged -= chart.OnCollectionChanged;
      if (e.NewValue is INotifyCollectionChanged newC)
         newC.CollectionChanged += chart.OnCollectionChanged;

      if (e.NewValue is IEnumerable items)
         foreach (var item in items.OfType<INotifyPropertyChanged>())
            item.PropertyChanged += chart.OnItemChanged;

      chart.Draw();
   }

   private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DonutChart)d).Draw();

   private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
   {
      if (e.NewItems != null)
         foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
            item.PropertyChanged += OnItemChanged;
      if (e.OldItems != null)
         foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
            item.PropertyChanged -= OnItemChanged;
      Draw();
   }

   private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
   {
      if (e.PropertyName == nameof(IDonutChartItem.Value))
         Draw();
   }

   private void Draw()
   {
      ChartCanvas.Children.Clear();
      if (ItemsSource == null!)
         return;

      var items = ItemsSource.OfType<IDonutChartItem>().ToList();
      var total = items.Sum(x => x.Value);

      CurrentTotal = total;
      if (total <= 0)
         return;

      var radius = (ChartSize - StrokeThickness) / 2;
      var center = new Point(ChartSize / 2, ChartSize / 2);
      double angle = 0;

      foreach (var item in items)
      {
         if (item.Value <= 0)
            continue;

         var pct = item.Value / total;
         var sweep = pct * 360;
         var drawSweep = sweep >= 360 ? 359.99 : (sweep > 1 ? sweep - 1 : sweep);

         var path = new Path
         {
            Stroke = item.ColorBrush,
            StrokeThickness = StrokeThickness,
            StrokeEndLineCap = PenLineCap.Flat,
            Data = CreateArc(center, radius, angle, angle + drawSweep),
            ToolTip = $"{item.Name}: {item.Value} ({pct:P0})",
         };
         ChartCanvas.Children.Add(path);
         angle += sweep;
      }
   }

   private static PathGeometry CreateArc(Point center, double radius, double startAngle, double endAngle)
   {
      var p1 = PolarToCartesian(center, radius, startAngle);
      var p2 = PolarToCartesian(center, radius, endAngle);
      var isLarge = (endAngle - startAngle) > 180.0;

      var fig = new PathFigure { StartPoint = p1, IsClosed = false };
      fig.Segments.Add(new ArcSegment(p2, new(radius, radius), 0, isLarge, SweepDirection.Clockwise, true));

      var geo = new PathGeometry();
      geo.Figures.Add(fig);
      return geo;
   }

   private static Point PolarToCartesian(Point center, double radius, double angleInDegrees)
   {
      var radians = (angleInDegrees - 90) * (Math.PI / 180.0);
      return new(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
   }
}