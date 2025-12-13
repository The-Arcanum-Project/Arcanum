using System.ComponentModel;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class SpecializedEditorSettings() : InternalSearchableSetting(Config.Settings)
{
   [Description("Settings specific to the POP Editor.")]
   public PopEditorSettings PopEditorSettings { get; set; } = new();
}