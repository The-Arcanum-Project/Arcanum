#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.CoreSystems.Nexus;
using Arcanum.Core.CoreSystems.Parsing.ParsingHelpers.ArcColor;
using Arcanum.Core.CoreSystems.Parsing.ParsingMaster;
using Arcanum.Core.CoreSystems.Parsing.Steps.InGame.Map;
using Arcanum.Core.CoreSystems.Queastor;
using Arcanum.Core.GameObjects.InGame.Map;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Utils.Colors;
using Arcanum.UI.Components.Windows.DebugWindows;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

#endregion

namespace Arcanum.UI.Components.Windows.MinorWindows.LocationColorPicker;

public sealed class LocationColorPickerViewModel : INotifyPropertyChanged
{
   private readonly HashSet<int> _usedColors;

   public enum DefaultMapOptions
   {
      None,
      Volcanoes,
      Earthquakes,
      SeaZones,
      Lakes,
      NotOwnable,
      ImpassableMountains,
   }

   public LocationColorPickerViewModel()
   {
      SuggestUnusedCommand = new RelayCommand(_ =>
      {
         SuggestUnused();
         CopyIsToggled();
      });
      CreateShadeCommand = new RelayCommand(_ =>
      {
         CreateShade(ReferenceColor);
         CopyIsToggled();
      });
      CreateShadeFromResultCommand = new RelayCommand(_ =>
      {
         CreateShade(SelectedColor);
         CopyIsToggled();
      });
      ConfirmCommand = new RelayCommand(_ => Confirm());
      CopyRgbCommand = new RelayCommand(_ => Clipboard.SetText(RgbText));
      CopyHexCommand = new RelayCommand(_ => Clipboard.SetText(HexText));

      _usedColors = Globals.Locations.Values.Select(x => x.Color.AsInt()).ToHashSet();
      (DescriptorDefinitions.LocationDescriptor.LoadingService[0] as LocationFileLoading)!.AddPlaceholders(_usedColors);
   }

   public Color SelectedColor
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         OnPropertyChanged(nameof(ColorBrush));
         OnPropertyChanged(nameof(RgbText));
         OnPropertyChanged(nameof(HexText));
      }
   } = Color.FromRgb(22, 87, 190);

   public Color ReferenceColor
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
         OnPropertyChanged(nameof(ReferenceBrush));
         OnPropertyChanged(nameof(ReferenceRgbText));
         OnPropertyChanged(nameof(ReferenceHexText));
      }
   } = Color.FromRgb(22, 87, 190);

   public float VariationMinLimit
   {
      get;
      set
      {
         if (value > VariationMaxLimit)
            value = VariationMaxLimit;
         field = value;
         OnPropertyChanged();
      }
   } = 2;

   public float VariationMaxLimit
   {
      get;
      set
      {
         if (value < VariationMinLimit)
            value = VariationMinLimit;
         field = value;
         OnPropertyChanged();
      }
   } = 100;

   public float VariationMin
   {
      get;
      set
      {
         var rounded = Math.Round(Math.Max(value, VariationMinLimit), 1);
         if (Math.Abs(field - rounded) < 0.01)
            return;

         field = (float)rounded;
         OnPropertyChanged();
      }
   } = 2;

   public float VariationMax
   {
      get;
      set
      {
         var rounded = Math.Round(Math.Min(value, VariationMaxLimit), 1);
         if (Math.Abs(field - rounded) < 0.01)
            return;

         field = (float)rounded;
         OnPropertyChanged();
      }
   } = 5;

   // Dropdown Selections
   public Province TargetProvince
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = Province.Empty;
   public Climate TargetClimate
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = Climate.Empty;
   public Topography TargetTopography
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = Topography.Empty;

   public bool IsCopyHex
   {
      get;
      set
      {
         field = value;
         CopyButtonText = IsCopyHex ? "Copy Hex" : "Copy RGB";
         OnPropertyChanged();
      }
   } = true;

   public bool AutoIncreaseNumber
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = true;

   public string CopyButtonText
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = "Copy Hex";

   public string NewLocationName
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = "";

   public DefaultMapOptions DefaultMapOption
   {
      get;
      set
      {
         field = value;
         OnPropertyChanged();
      }
   } = DefaultMapOptions.None;

   public SolidColorBrush ColorBrush => new(SelectedColor);
   public SolidColorBrush ReferenceBrush => new(ReferenceColor);
   public string RgbText => $"{SelectedColor.R} {SelectedColor.G} {SelectedColor.B}";
   public string HexText => $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
   public string ReferenceRgbText => $"{ReferenceColor.R} {ReferenceColor.G} {ReferenceColor.B}";
   public string ReferenceHexText => $"#{ReferenceColor.R:X2}{ReferenceColor.G:X2}{ReferenceColor.B:X2}";

   public ICommand SuggestUnusedCommand { get; }
   public ICommand CreateShadeCommand { get; }
   public ICommand CreateShadeFromResultCommand { get; }
   public ICommand ConfirmCommand { get; }
   public ICommand CopyRgbCommand { get; }
   public ICommand CopyHexCommand { get; }
   public DefaultMapOptions[] MapAssignmentOptions { get; } = Enum.GetValues<DefaultMapOptions>();

   public event PropertyChangedEventHandler? PropertyChanged;

   private void CopyIsToggled()
   {
      var textToCopy = IsCopyHex ? HexText : RgbText;

      Task.Run(() =>
      {
         for (var i = 0; i < 15; i++)
            try
            {
               // Application.Current.Dispatcher ensures we talk to the clipboard correctly.
               Application.Current.Dispatcher.Invoke(() => { Clipboard.SetText(textToCopy); });
               return;
            }
            catch (COMException ex) when ((uint)ex.ErrorCode == 0x800401D0)
            {
               Thread.Sleep(30);
            }
      });
   }

   private void SuggestUnused()
   {
      SelectedColor = ColorGenerator.GenerateUnusedColor(_usedColors, true);
   }

   private void CreateShade(Color referenceColor)
   {
      SelectedColor = ColorGenerator.GenerateUnusedShade(_usedColors, referenceColor, VariationMin, VariationMax);
   }

   private void Confirm()
   {
      var newColor = new JominiColor.MediaColor(SelectedColor);
      var err = string.Empty;
      if (TargetProvince == Province.Empty)
         err += "No Province selected.\n";
      if (TargetClimate == Climate.Empty)
         err += "No Climate selected.\n";
      if (TargetTopography == Topography.Empty)
         err += "No Topography selected.\n";
      if (string.IsNullOrEmpty(NewLocationName))
         err += "No Location name entered.\n";
      if (_usedColors.Contains(newColor.AsInt()))
         err += $"Selected color is already in use ({newColor}.\n";
      
      if (!string.IsNullOrEmpty(err))
      {
         MBox.Show(err, "Invalid Input", MBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      var newLocation = Eu5ObjectCreator.ShowOnlyNamePickingPopUp(typeof(Location), _ => { }, NewLocationName);
      IncrementNameSuffix();
      
      if (newLocation == null)
         return;

      if (string.IsNullOrEmpty(newLocation.UniqueId))
      {
         MBox.Show("Invalid Location name.", "Error", MBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      var template = new LocationTemplateData();
      Nx.Set(template, LocationTemplateData.Field.Climate, TargetClimate);
      Nx.Set(template, LocationTemplateData.Field.Topography, TargetTopography);
      Nx.Set(newLocation, Location.Field.TemplateData, template);
      Nx.AddToCollection(TargetProvince, Province.Field.Locations, newLocation);
      ((Location)newLocation).ColorIndex = LocationFileLoading.ColorIndex++;
      Nx.Set(newLocation, Location.Field.Color, newColor);
      _usedColors.Add(newColor.AsInt());
      

      template.UniqueId = newLocation.UniqueId;
      Globals.LocationTemplateData.Add(template.UniqueId, template);
      Queastor.GlobalInstance.AddToIndex(template);

      AppendToDefaultMap(DefaultMapOption, (Location)newLocation);

      AppendDefinition((Location)newLocation);
   }

   public void IncrementNameSuffix()
   {
      var input = NewLocationName;
      if (string.IsNullOrEmpty(input))
         return;

      var lastUnderscore = input.LastIndexOf('_');

      // Check if there is an underscore and at least one character after it
      if (lastUnderscore == -1 || lastUnderscore == input.Length - 1)
         return;

      // Get the part after the underscore
      var suffix = input.AsSpan(lastUnderscore + 1);

      // Verify all characters in the suffix are digits
      for (var i = 0; i < suffix.Length; i++)
         if (!char.IsDigit(suffix[i]))
            return;

      if (int.TryParse(suffix, out var number))
         NewLocationName = string.Concat(input.AsSpan(0, lastUnderscore + 1), (number + 1).ToString());
   }

   private static void AppendDefinition(Location newLocation)
   {
      var files = DescriptorDefinitions.LocationDescriptor.Files;
      if (files.Count == 0)
         throw new InvalidOperationException("No location files found to append to.");

      var fileObj = files[^1];

      var oldPath = fileObj.GetFullPath();
      var moved = false;
      if (!fileObj.IsModded && Config.Settings.SavingConfig.MoveFilesToModdedDataSpaceOnSaving)
      {
         fileObj.Path.MoveToMod();
         moved = true;
      }

      fileObj.Path.UnregisterWatcher();
      if (moved)
         IO.CopyTo(oldPath, fileObj.GetFullPath());
      IO.WriteAllTextUtf8WithBom(fileObj.GetFullPath(), $"\n{newLocation.UniqueId} = {newLocation.Color.AsHexString()}", true);
      fileObj.Path.RegisterWatcher();
   }

   private static void AppendToDefaultMap(DefaultMapOptions option, Location loc)
   {
      switch (option)
      {
         case DefaultMapOptions.None:
            break;
         case DefaultMapOptions.Volcanoes:
            Nx.AddToCollection(Globals.DefaultMapDefinition, DefaultMapDefinition.Field.Volcanoes, loc);
            break;
         case DefaultMapOptions.Earthquakes:
            Nx.AddToCollection(Globals.DefaultMapDefinition, DefaultMapDefinition.Field.Earthquakes, loc);
            break;
         case DefaultMapOptions.SeaZones:
            Nx.AddToCollection(Globals.DefaultMapDefinition, DefaultMapDefinition.Field.SeaZones, loc);
            break;
         case DefaultMapOptions.Lakes:
            Nx.AddToCollection(Globals.DefaultMapDefinition, DefaultMapDefinition.Field.Lakes, loc);
            break;
         case DefaultMapOptions.NotOwnable:
            Nx.AddToCollection(Globals.DefaultMapDefinition, DefaultMapDefinition.Field.NotOwnable, loc);
            break;
         case DefaultMapOptions.ImpassableMountains:
            Nx.AddToCollection(Globals.DefaultMapDefinition, DefaultMapDefinition.Field.ImpassableMountains, loc);
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(option), option, null);
      }
   }

   private void OnPropertyChanged([CallerMemberName] string name = null!) => PropertyChanged?.Invoke(this, new(name));
}