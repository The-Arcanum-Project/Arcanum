using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.CoreSystems.NUI.NUIUserControls;
using Arcanum.Core.GlobalStates;
using Common.UI.NUI;
using Color = System.Windows.Media.Color;

namespace Arcanum.Core.GameObjects.Pops;

public partial class PopType(string name,
                             string colorKey,
                             float foodConsumption,
                             float assimilationConversionFactor) : IParseable<PopType>, INUI
{
   public string Name { get; } = name;
   public string ColorKey { get; } = colorKey;
   public float FoodConsumption { get; } = foodConsumption;
   public float AssimilationConversionFactor { get; } = assimilationConversionFactor;

   public override string ToString()
   {
      return $"{Name} ({ColorKey})";
   }

   public static PopType Empty { get; } = new(string.Empty, string.Empty, 0f, 0f);

   public bool Parse(string? str, out PopType? result)
   {
      if (Globals.PopTypes.TryGetValue(str ?? string.Empty, out result))
         return true;

      DiagnosticException.LogWarning(LocationContext.Empty,
                                     ParsingError.Instance.InvalidPopTypeKey,
                                     nameof(Parse).GetType().FullName!,
                                     str ?? "null");

      result = Empty;
      return false;
   }

   public override bool Equals(object? obj)
   {
      if (obj is PopType other)
         return Name == other.Name;

      return false;
   }

   public override int GetHashCode()
   {
      return Name.GetHashCode();
   }

   public static bool operator ==(PopType? left, PopType? right)
   {
      if (left is null && right is null)
         return true;

      if (left is null || right is null)
         return false;

      return left.Equals(right);
   }

   public static bool operator !=(PopType? left, PopType? right)
   {
      return !(left == right);
   }

   public event PropertyChangedEventHandler? PropertyChanged;
   public NUIUserControl GetObjectView(Enum[]? e = null) => throw new NotImplementedException();

   public NUIUserControl GetEmbeddedView(Enum[] e)
   {
      var defaultNui = new DefaultNUI(this, ViewType.EmbeddedView)
      {
         DataContext = this,
         Content = new Border()
         {
            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 66, 66)), BorderThickness = new(5),
         }
      };
      return defaultNui;
   }

   public NUIUserControl GetEmbeddedEditorView(Enum[]? e = null) => throw new NotImplementedException();

   public NUIUserControl GetShortInfoView(Enum[]? e = null) => throw new NotImplementedException();

   public KeyValuePair<string, string> GetTitleAndSubTitle() => throw new NotImplementedException();

   public NUIUserControl GetBaseUI(ViewType view) => new DefaultNUI(this, view) { DataContext = this };
}