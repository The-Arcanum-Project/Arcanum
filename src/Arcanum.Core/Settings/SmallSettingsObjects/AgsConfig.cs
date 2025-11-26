using System.ComponentModel;
using Arcanum.Core.GameObjects.BaseTypes.InjectReplace;

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

   [Description("Number of spaces to use for each indentation level in the saved AGS files.")]
   [DefaultValue(3)]
   public int SpacesPerIndent { get; set; } = 3;

   [Description("Whether to use INJECT/REPLACE calls when saving objects.")]
   [DefaultValue(true)]
   public bool UseInjectReplaceCalls { get; set; } = true;

   [Description("Maximum percentage of properties to INJECT into an object before an REPLACE is used instead.")]
   [DefaultValue(0.5f)]
   public float MaxPercentageToInject { get; set; } = 0.5f;

   [Description("Default INJECT behavior when none is specified.")]
   [DefaultValue(InjRepType.INJECT)]
   public InjRepType DefaultInjectType { get; set; } = InjRepType.INJECT;

   [Description("Default REPLACE behavior when none is specified.")]
   [DefaultValue(InjRepType.REPLACE)]
   public InjRepType DefaultReplaceType { get; set; } = InjRepType.REPLACE;

   [Description("The strategy to use in case there already is an inject in a base mod for the value being injected.\n Recommended to keep as Inject.")]
   [DefaultValue(InjRepType.INJECT)]
   public InjRepType InjectCollisionResolveStrategy { get; set; } = InjRepType.INJECT;

   [Description("If true, when saving objects to files, files will be moved to the modded data space.")]
   [DefaultValue(true)]
   public bool MoveFilesToModdedDataSpaceOnSaving { get; set; } = true;

   [Description("If true, the setup folder will save all e.g. location specific data into a single file. If false, separate files will be used as in vanilla.")]
   [DefaultValue(true)]
   public bool CompactSetupFolder { get; set; } = true;
}