using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Arcanum.API;
using Arcanum.API.Events;
using Arcanum.API.Settings;
using Arcanum.Core.Utils.DelayedEvents;
using Arcanum.UI.Components.UserControls;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Util;

namespace Arcanum.UI.HostUIServices.SettingsGUI;

public partial class PluginSettingsWindow
{
   private readonly List<PropertyGrid> _propertyGrids = [];

   public PluginSettingsWindow()
   {
      InitializeComponent();

      ResetSelected.Click += (_, _) => { ResetPropertyToDefault(FindPropertyGrid()); };

      ResetAll.Click += (_, _) =>
      {
         var propGrid = FindPropertyGrid();
         foreach (var item in propGrid.Properties)
            if (item.PropertyInfo is { } info)
               ResetPropertyToDefault(propGrid, info);
      };
   }

   public static void ShowSettingsWindow(Dictionary<Guid, IPluginSetting> settings, Guid focusOnGuid, IPluginHost host)
   {
      var psw = new PluginSettingsWindow();
      psw.SetSettings(settings, host);
      psw.FocusOnTab(focusOnGuid);
      psw.ShowDialog();
   }

   private void PropertySelection_Changed(object? sender, SelectionChangedEventArgs e)
   {
      if (sender is not PropertyGrid propGrid)
         return;

      // If the DefaultValue attribute is present, enable the reset button
      if (propGrid.SelectedPropertyItem?.PropertyInfo is { } info)
         ResetSelected.IsEnabled = null != info.GetCustomAttribute<DefaultValueAttribute>();
   }

   public void SetSettings(Dictionary<Guid, IPluginSetting> settings, IPluginHost host)
   {
      foreach (var (plugin, settingObj) in settings)
      {
         var propGrid = new PropertyGrid
         {
            Margin = new(0), SelectedObject = settingObj,
         };
         propGrid.PropertySelected += PropertySelection_Changed;
         propGrid.PropertyValueChanged.AddHandler((_, e) => { HandleSettingChangeNotification(host, e, propGrid); });

         SettingsTabControl.Items.Add(new TabItem
         {
            Header = host.GuidToName(plugin),
            Tag = plugin,
            Content = GetTabItemGrid(propGrid),
         });

         _propertyGrids.Add(propGrid);
      }
   }

   private PropertyGrid FindPropertyGrid()
   {
      foreach (var propGrid in _propertyGrids)
         if (IsControlOnSelectedTab(SettingsTabControl, propGrid))
            return propGrid;

      throw new InvalidOperationException("No PropertyGrid found.");
   }

   private bool IsControlOnSelectedTab(TabControl tabControl, UIElement control)
   {
      if (tabControl.SelectedItem is not TabItem selectedTab)
         return false;

      return FindChildrenUtil.IsDescendantOf(control, selectedTab.Content as DependencyObject);
   }

   private static Grid GetTabItemGrid(PropertyGrid propGrid)
   {
      var grid = new Grid
      {
         Margin = new(0),
         Background = Brushes.Transparent,
         HorizontalAlignment = HorizontalAlignment.Stretch,
         VerticalAlignment = VerticalAlignment.Stretch,
         ColumnDefinitions =
         {
            new() { Width = new(5, GridUnitType.Pixel) }, new() { Width = new(1, GridUnitType.Star) },
         }
      };

      var separator = new Border
      {
         Background = (Brush)Application.Current.FindResource("DefaultBorderColorBrush")!,
         Width = 3,
         VerticalAlignment = VerticalAlignment.Stretch,
         HorizontalAlignment = HorizontalAlignment.Left,
         CornerRadius = new(3),
         Margin = new(0, 2, 2, 3),
      };

      grid.Children.Add(separator);
      grid.Children.Add(propGrid);
      Grid.SetColumn(separator, 0);
      Grid.SetColumn(propGrid, 1);

      return grid;
   }

   private void HandleSettingChangeNotification(IPluginHost host,
                                                PropertyValueChangedEventArgs e,
                                                PropertyGrid propGrid)
   {
      if (e.ChangedItem != null)
      {
         Dispatcher.BeginInvoke(() =>
         {
            MessageBox.Show($"Property '{e.ChangedItem.PropertyInfo.Name}' changed.\n" +
                            $"Old Value: {e.OldValue ?? "null"}\n" +
                            $"New Value: {e.ChangedItem.Value}");
         });
      }

      // Notify the host about the setting change
      var eventArgs = new PluginSettingEventArgs(((IPluginSetting)propGrid.SelectedObject!).OwnerGuid,
                                                 EventSource.User,
                                                 propGrid.SelectedObject.GetType()
                                                         .GetProperty(e.ChangedItem?.PropertyInfo.Name ??
                                                                      string.Empty),
                                                 e.ChangedItem?.Value);
      host.GetService<IEventBus>().Trigger(PluginEventId.Settings_OnSettingChanged, eventArgs);
   }

   private static void ResetPropertyToDefault(PropertyGrid propGrid)
   {
      if (propGrid.SelectedPropertyItem?.PropertyInfo is { } info)
         ResetPropertyToDefault(propGrid, info);
   }

   private static void ResetPropertyToDefault(PropertyGrid propGrid, PropertyInfo info)
   {
      var defaultValueAttribute = info.GetCustomAttribute<DefaultValueAttribute>();
      if (defaultValueAttribute == null)
         return;

      info.SetValue(propGrid.SelectedObject, defaultValueAttribute.Value);

      propGrid.UpdatePropertyItem();
   }

   public void FocusOnTab(Guid pluginGuid)
   {
      if (pluginGuid == Guid.Empty)
         return;

      TabItem? first = null;
      foreach (TabItem tabItem in SettingsTabControl.Items)
      {
         first ??= tabItem;
         if (tabItem.Tag is Guid tag && tag.Equals(pluginGuid))
         {
            tabItem.Focus();
            return;
         }
      }

      first?.Focus();
   }
}