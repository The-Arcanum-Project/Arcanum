using System.ComponentModel;
using System.Windows.Media;
using Arcanum.Core.Utils;

namespace Arcanum.UI.Components.UserControls.BaseControls;

public sealed class ColorPickerViewModel : INotifyPropertyChanged
{
   private bool _isUpdating;
   private Color _selectedColor;
   private double _hue;
   private double _saturation;
   private double _value;
   private byte _red;
   private byte _green;
   private byte _blue;

   private string _hexText = string.Empty;

   public ColorPickerViewModel()
   {
      SelectedColor = Colors.Red;
   }

   public Color SelectedColor
   {
      get => _selectedColor;
      set
      {
         if (_selectedColor == value)
            return;

         UpdateAllPropertiesFromColor(value);
      }
   }

   public double Hue
   {
      get => _hue;
      set
      {
         if (Math.Abs(_hue - value) < 0.01)
            return;

         _hue = value;
         OnPropertyChanged(nameof(Hue));
         UpdatePropertiesFromHsv();
      }
   }

   public double Saturation
   {
      get => _saturation;
      set
      {
         if (Math.Abs(_saturation - value) < 0.01)
            return;

         _saturation = value;
         OnPropertyChanged(nameof(Saturation));
         UpdatePropertiesFromHsv();
      }
   }

   public double Value
   {
      get => _value;
      set
      {
         if (Math.Abs(_value - value) < 0.01)
            return;

         _value = value;
         OnPropertyChanged(nameof(Value));
         UpdatePropertiesFromHsv();
      }
   }

   public byte Red
   {
      get => _red;
      set
      {
         if (_red == value)
            return;

         _red = value;
         OnPropertyChanged(nameof(Red));
         UpdatePropertiesFromRgb(_red, _green, _blue);
      }
   }

   public byte Green
   {
      get => _green;
      set
      {
         if (_green == value)
            return;

         _green = value;
         OnPropertyChanged(nameof(Green));
         UpdatePropertiesFromRgb(_red, _green, _blue);
      }
   }

   public byte Blue
   {
      get => _blue;
      set
      {
         if (_blue == value)
            return;

         _blue = value;
         OnPropertyChanged(nameof(Blue));
         UpdatePropertiesFromRgb(_red, _green, _blue);
      }
   }

   public string HexText
   {
      get => _hexText;
      set
      {
         if (_hexText == value)
            return;

         _hexText = value;
         OnPropertyChanged(nameof(HexText));

         var parsableHex = value.TrimStart('#');

         try
         {
            if (parsableHex.Length == 6)
            {
               var newColor = (Color)ColorConverter.ConvertFromString("#" + parsableHex);
               SelectedColor = newColor;
            }
            else if (parsableHex.Length == 3 && IsValidHex(parsableHex))
            {
               var expandedHex = $"#{parsableHex[0]}{parsableHex[0]}" +
                                 $"{parsableHex[1]}{parsableHex[1]}" +
                                 $"{parsableHex[2]}{parsableHex[2]}";

               var newColor = (Color)ColorConverter.ConvertFromString(expandedHex);

               UpdateColorWithoutChangingHexText(newColor);
            }
         }
         catch (FormatException)
         {
         }
      }
   }

   private bool IsValidHex(string s)
   {
      return s.All(c => "0123456789abcdefABCDEF".Contains(c));
   }

   private void UpdateColorWithoutChangingHexText(Color color)
   {
      if (_isUpdating)
         return;

      _isUpdating = true;

      _selectedColor = color;
      var (h, s, v) = ColorConversion.RgbToHsv(color.R, color.G, color.B);

      _hue = h;
      _saturation = s;
      _value = v;
      _red = color.R;
      _green = color.G;
      _blue = color.B;
      // _hexText = ...; // <-- DO NOT update this.

      OnPropertyChanged(nameof(SelectedColor));
      OnPropertyChanged(nameof(Hue));
      OnPropertyChanged(nameof(Saturation));
      OnPropertyChanged(nameof(Value));
      OnPropertyChanged(nameof(Red));
      OnPropertyChanged(nameof(Green));
      OnPropertyChanged(nameof(Blue));
      // OnPropertyChanged(nameof(HexText)); // DO NOT notify this.
      OnPropertyChanged(nameof(PureHueColor));

      _isUpdating = false;
   }

   private void UpdateAllPropertiesFromColor(Color color)
   {
      if (_isUpdating)
         return;

      _isUpdating = true;

      _selectedColor = color;
      var (h, s, v) = ColorConversion.RgbToHsv(color.R, color.G, color.B);

      _hue = h;
      _saturation = s;
      _value = v;
      _red = color.R;
      _green = color.G;
      _blue = color.B;
      _hexText = $"{color.R:X2}{color.G:X2}{color.B:X2}";

      // Notify UI of all changes
      OnPropertyChanged(nameof(SelectedColor));
      OnPropertyChanged(nameof(Hue));
      OnPropertyChanged(nameof(Saturation));
      OnPropertyChanged(nameof(Value));
      OnPropertyChanged(nameof(Red));
      OnPropertyChanged(nameof(Green));
      OnPropertyChanged(nameof(Blue));
      OnPropertyChanged(nameof(HexText));
      OnPropertyChanged(nameof(PureHueColor));

      _isUpdating = false;
   }

   public Color PureHueColor => ColorConversion.HsvToRgb(_hue, 1, 1);

   private void UpdatePropertiesFromHsv()
   {
      if (_isUpdating)
         return;

      SelectedColor = ColorConversion.HsvToRgb(_hue, _saturation, _value);
   }

   private void UpdatePropertiesFromRgb(byte r, byte g, byte b)
   {
      if (_isUpdating)
         return;

      SelectedColor = Color.FromRgb(r, g, b);
   }

   public event PropertyChangedEventHandler? PropertyChanged;

   private void OnPropertyChanged(string propertyName)
   {
      PropertyChanged?.Invoke(this, new(propertyName));
   }
}