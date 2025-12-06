using System.Windows.Input;
using System.Windows.Media;
using Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.Components.UserControls.ValueAllocators;

public class AllocationItem : ViewModelBase
{
   private readonly AllocatorViewModel _parent;
   private int _value;
   private string _name;
   private bool _isLocked;
   private Color _mediaColor;
   private int _minLimit;
   private int _maxLimit;
   private SolidColorBrush _colorBrush;

   public ICommand IncrementCommand { get; }
   public ICommand DecrementCommand { get; }

   public Color MediaColor
   {
      get => _mediaColor;
      set
      {
         _mediaColor = value;
         UpdateBrush(); // Regenerate brush if color changes
         OnPropertyChanged();
      }
   }

   // The UI-ready Brush (Reduces XAML complexity)
   public SolidColorBrush ColorBrush
   {
      get => _colorBrush;
      private set
      {
         _colorBrush = value;
         OnPropertyChanged();
      }
   }

   public string Name
   {
      get => _name;
      set
      {
         _name = value;
         OnPropertyChanged();
      }
   }

   public bool IsLocked
   {
      get => _isLocked;
      set
      {
         _isLocked = value;
         OnPropertyChanged();
         OnPropertyChanged(nameof(IsNotLocked)); // Helper for XAML binding
      }
   }

   public bool IsNotLocked => !_isLocked;

   public double SliderMinPosition => ConvertToSliderScale(_minLimit);

   public double SliderMaxRemaining
   {
      get
      {
         double t = _parent.TotalLimit;
         double maxPos = ConvertToSliderScale(_maxLimit);
         return (ConvertToSliderScale((int)t) - maxPos);
      }
   }

   public int MinLimit
   {
      get => _minLimit;
      set
      {
         // If user types a Min > Current Max, do we cap it? Or reject it?
         // Clamping is usually safer for UI binding loops.
         if (value > _maxLimit)
            value = _maxLimit;
         if (value < 0)
            value = 0;

         if (_minLimit != value)
         {
            _minLimit = value;
            OnPropertyChanged();
            RefreshSlider();

            // Force value compliance
            if (Value < _minLimit)
               Value = _minLimit;
         }
      }
   }

   public int MaxLimit
   {
      get => _maxLimit;
      set
      {
         // If user types Max < Current Min
         if (value < _minLimit)
            value = _minLimit;
         if (value > _parent.TotalLimit)
            value = _parent.TotalLimit;

         if (_maxLimit != value)
         {
            _maxLimit = value;
            OnPropertyChanged();
            RefreshSlider();

            // Force value compliance
            if (Value > _maxLimit)
               Value = _maxLimit;
         }
      }
   }

   public int Value
   {
      get => _value;
      set
      {
         if (_value != value)
         {
            if (IsLocked)
               _parent.UpdateLockedItem(this, value);
            else
               _parent.UpdateItem(this, value);
         }
      }
   }

   public double SliderValue
   {
      get
      {
         if (!_parent.IsLogarithmic)
            return _value;

         double t = _parent.TotalLimit;
         if (t <= 0)
            return 0;

         return t * (Math.Log(1 + _value) / Math.Log(1 + t));
      }
      set
      {
         if (IsLocked)
            return;

         int intVal = (int)Math.Round(value);
         if (_parent.IsLogarithmic)
         {
            double t = _parent.TotalLimit;
            if (t > 0)
            {
               double computed = Math.Pow(1 + t, value / t) - 1;
               intVal = (int)Math.Round(computed);
            }
         }

         if (intVal < MinLimit)
            intVal = MinLimit;
         if (intVal > MaxLimit)
            intVal = MaxLimit;

         Value = intVal;
         OnPropertyChanged();
      }
   }

   public string PercentageDisplay
   {
      get
      {
         if (_parent.TotalLimit == 0)
            return "0%";

         return $"{((double)_value / _parent.TotalLimit) * 100:F0}%";
      }
   }

   public AllocationItem(AllocatorViewModel parent, string name, int val, Color color, int min = 0, int max = int.MaxValue)
   {
      _parent = parent;
      _name = name;
      _value = val;
      _isLocked = false;
      _minLimit = min;
      _maxLimit = max;

      _mediaColor = new()
      {
         A = 120,
         R = color.R,
         G = color.G,
         B = color.B,
      };

      // Init Commands
      IncrementCommand = new RelayCommand(Increment);
      DecrementCommand = new RelayCommand(Decrement);

      UpdateBrush();
   }

   private double ConvertToSliderScale(int val)
   {
      if (!_parent.IsLogarithmic)
         return val;

      double t = _parent.TotalLimit;
      if (t <= 0)
         return 0;

      return t * (Math.Log(1 + val) / Math.Log(1 + t));
   }

   private void Increment()
   {
      // If Locked: Just add 1 (Total grows)
      // If Unlocked: Add 1 (Others shrink), but cap at TotalLimit
      // The Value setter handles logic, we just check bounds here.

      if (!IsLocked && Value >= _parent.TotalLimit)
         return;

      Value++;
   }

   private void Decrement()
   {
      if (Value <= 0)
         return;

      Value--;
   }

   private void UpdateBrush()
   {
      // Create a frozen brush for performance/thread-safety
      var brush = new SolidColorBrush(_mediaColor);
      brush.Freeze();
      ColorBrush = brush;
   }

   public void SetValueInternal(int val)
   {
      if (val < 0)
         val = 0;
      _value = val;
      RefreshSlider();
   }

   public void RefreshSlider()
   {
      OnPropertyChanged(nameof(Value));
      OnPropertyChanged(nameof(SliderValue));
      OnPropertyChanged(nameof(PercentageDisplay));

      OnPropertyChanged(nameof(SliderMinPosition));
      OnPropertyChanged(nameof(SliderMaxRemaining));
   }
}