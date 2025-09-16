using System.ComponentModel;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class AgsConfig
{
   /// <summary>
   /// If true, when saving AGS files, all properties will be written, even if they have default values.
   /// If false, only properties that differ from their default values will be written.
   /// </summary>
   [Description("If true, when saving AGS files, all properties will be written, even if they have default values.\n If false, only properties that differ from their default values will be written.")]
   [DefaultValue(false)]
   public bool WriteAllDefaultValues { get; set; } = false;
}