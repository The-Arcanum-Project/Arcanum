using System.ComponentModel;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class MiscSettings
{
   [DefaultValue("{x#.###}, {y#.###}")]
   [Description("Formats coordinates with custom precision ({x#.#} = 1 decimal) or default .NET styles—like {y:F0}. Maps any {var:#format} to numbered output.")]
   public string CustomCoordinatesFormat { get; set; } = "{x#.###}, {y#.###}";
}