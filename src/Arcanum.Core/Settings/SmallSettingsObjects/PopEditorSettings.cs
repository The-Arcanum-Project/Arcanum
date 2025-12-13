using System.ComponentModel;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class PopEditorSettings() : InternalSearchableSetting(Config.Settings)
{
   [Description("The factor by which to multiply total a location already has to set the max for the slider.")]
   [DefaultValue(3)]
   public int TotalPopsFactor
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 3;

   [Description("Pop creation random offset percentage. Higher values will create more varied pop sizes.")]
   [DefaultValue(0.1f)]
   public float PopCreationRandomOffsetPercentage
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 0.1f;

   [Description("The default size for newly created pops if no better guess can be made.")]
   [DefaultValue(100)]
   public int DefaultPopSize
   {
      get;
      set => SetNotifyProperty(ref field, value);
   } = 100;
}