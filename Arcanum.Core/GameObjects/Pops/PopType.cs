using System.ComponentModel;
using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.ErrorSystem.BaseErrorTypes;
using Arcanum.Core.CoreSystems.ErrorSystem.Diagnostics;
using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GlobalStates;

namespace Arcanum.Core.GameObjects.Pops;

public partial class PopType(string name,
                             string colorKey,
                             float foodConsumption,
                             float assimilationConversionFactor) : IParseable<PopType>, INUI
{
   public string Name { get; set; } = name;
   public string ColorKey { get; set; } = colorKey;
   public float FoodConsumption { get; set; } = foodConsumption;
   public float AssimilationConversionFactor { get; set; } = assimilationConversionFactor;

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

   public bool IsReadonly { get; } = false;
   public NUISetting Settings { get; } = Config.Settings.NUISettings.PopTypeSettings;
   public INavigate[] Navigations { get; } = [];
}