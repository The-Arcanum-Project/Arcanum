#region

using Arcanum.UI.Components.UserControls.Map;
using CommunityToolkit.Mvvm.ComponentModel;

#endregion

namespace Arcanum.UI.Components.Views.MainWindow;

public class EasterEgg2026 : ObservableObject
{
   public EasterEgg2026()
   {
      if (IsAprilFoolsTimeFrame)
      {
         Enabled = true;
         StateEnum = EasterEgg2026State.Standard;
         ApplyChanges();
      }
   }

   public bool Enabled
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
         ApplyChanges();
      }
   }

   // One week of April Fools
   public static bool IsAprilFoolsTimeFrame
   {
      get
      {
         var now = DateTime.Now;
         return now is { Month: 4, Day: >= 1 and <= 7 };
      }
   }

   public EasterEgg2026State StateEnum
   {
      get;
      set
      {
         if (value == field)
            return;

         field = value;
         OnPropertyChanged();
         if (Enabled)
            ApplyChanges();
      }
   }
   public static Array EasterEggStates => Enum.GetValues<EasterEgg2026State>();
   private MapControl? _mapControl;

   public void SetMap(MapControl mainMap)
   {
      _mapControl = mainMap;
   }

   public void ApplyChanges()
   {
      _mapControl?.SetMapEffect((int)StateEnum, Enabled);
   }
}

// 0: Standard (Alpha channel only)
// 1: Popcorn (Individual wiggle)
// 2: Ripple (Wave from mouse)
// 3: Magnet (Look at mouse)
// 4: Swirl (Vortex)
public enum EasterEgg2026State
{
   Standard = 0,
   Popcorn = 1,
   Ripple = 2,
   Magnet = 3,
   Swirl = 4,
}