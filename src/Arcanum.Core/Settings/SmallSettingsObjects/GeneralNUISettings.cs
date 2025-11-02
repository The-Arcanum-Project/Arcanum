using System.ComponentModel;
using Arcanum.Core.Settings.BaseClasses;

namespace Arcanum.Core.Settings.SmallSettingsObjects;

public class GeneralNUISettings() : InternalSearchableSetting(Config.Settings)
{
   private bool _startEmbeddedFieldsCollapsed = true;
   private bool _listViewsInCustomOrder = true;
   private bool _disableNUIInferFromMapActions;
   private int _maxItemsMovedWithoutWarning = 10;
   private int _maxCollectionItemsPreviewed = 5;
   [Description("Maximum number of items in a collection to preview in the NUI")]
   [DefaultValue(5)]
   public int MaxCollectionItemsPreviewed
   {
      get => _maxCollectionItemsPreviewed;
      set => SetNotifyProperty(ref _maxCollectionItemsPreviewed, value);
   }

   [Description("Maximum number of items that can be moved at once without a warning if you really want to move them")]
   [DefaultValue(10)]
   public int MaxItemsMovedWithoutWarning
   {
      get => _maxItemsMovedWithoutWarning;
      set => SetNotifyProperty(ref _maxItemsMovedWithoutWarning, value);
   }

   [Description("If true, NUI will not not generate map actions to set properties via map inference")]
   [DefaultValue(false)]
   public bool DisableNUIInferFromMapActions
   {
      get => _disableNUIInferFromMapActions;
      set => SetNotifyProperty(ref _disableNUIInferFromMapActions, value);
   }

   [Description("If true, lists of views will be shown in the custom order defined in the NUI settings, otherwise alphabetically")]
   [DefaultValue(true)]
   public bool ListViewsInCustomOrder
   {
      get => _listViewsInCustomOrder;
      set => SetNotifyProperty(ref _listViewsInCustomOrder, value);
   }

   [Description("If true, embedded fields will start collapsed by default")]
   [DefaultValue(true)]
   public bool StartEmbeddedFieldsCollapsed
   {
      get => _startEmbeddedFieldsCollapsed;
      set => SetNotifyProperty(ref _startEmbeddedFieldsCollapsed, value);
   }
}