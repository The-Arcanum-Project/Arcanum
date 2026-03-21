using System.Numerics;

namespace Arcanum.Core.CoreSystems.Map.MapModes;

/// <summary>
/// Defines the contract for Arcanum.UI to generate context menu options for map modes dynamically based on the current map mode and selected locations.
/// </summary>
public struct MapContexMenuConfig
{
   public bool IsEnabled { get; set; }
   public string OptionName { get; set; }
   public Func<Vector2, string> Tooltip { get; set; }
   public Action<Vector2> OptionAction { get; set; }
}