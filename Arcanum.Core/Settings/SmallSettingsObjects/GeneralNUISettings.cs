using System.ComponentModel;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class GeneralNUISettings
{
   [Description("Maximum number of items in a collection to preview in the NUI")]
   [DefaultValue(5)]
   public int MaxCollectionItemsPreviewed { get; set; } = 5;
   
   [Description("Maximum number of items that can be moved at once without a warning if you really want to move them")]
   [DefaultValue(10)]
   public int MaxItemsMovedWithoutWarning { get; set; } = 10;
   
   [Description("If true, NUI will not not generate map actions to set properties via map inference")]
   [DefaultValue(false)]
   public bool DisableNUIInferFromMapActions { get; set; } = false;
}