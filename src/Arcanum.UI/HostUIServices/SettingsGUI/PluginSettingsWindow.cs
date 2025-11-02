// using System.ComponentModel;
// using Arcanum.SDK;
// using Arcanum.SDK.Events;
// using Arcanum.SDK.AgsSettings;
// using Arcanum.UI.GeneralUiElements.DarkUiElements;
//
// namespace Arcanum.UI.HostUIServices.SettingsGUI;
//
// public class SettingsUiService : ISettingsUiService
// {
//    public EventHandler<InputConfirmEventArgs> SettingChanged { get; set; } = delegate { };
//
//    public void ShowSettingsWindow(Dictionary<Guid, IPluginSetting> settings, Guid focusOnGuid, IPluginHost host)
//    {
//       var psw = new PluginSettingsWindow();
//       psw.IsReady = false;
//       psw.SetSettings(settings, host);
//       psw.IsReady = true;
//       psw.FocusOnTab(focusOnGuid);
//       psw.ShowDialog();
//    }
// }
//
// public partial class PluginSettingsWindow : Form
// {
//    public bool IsReady { get; set; }
//    private DarkTabControl SettingsTab { get; } = new()
//    {
//       Dock = DockStyle.Fill,
//       SizeMode = TabSizeMode.Fixed,
//       Alignment = TabAlignment.Left,
//    };
//
//    public PluginSettingsWindow()
//    {
//       InitializeComponent();
//
//       Controls.Add(SettingsTab);
//
//       SetCustomTabWidth();
//    }
//
//    public void SetSettings(Dictionary<Guid, IPluginSetting> settings, IPluginHost host)
//    {
//       foreach (var (plugin, settingObj) in settings)
//       {
//          var propGrid = new PropertyGrid
//          {
//             Dock = DockStyle.Fill,
//             SelectedObject = settingObj,
//             PropertySort = PropertySort.CategorizedAlphabetical,
//             LineColor = SystemColors.ControlDark,
//          };
//          propGrid.PropertyValueChanged += (_, e) =>
//          {
//             // Notify the host about the setting change
//             var eventArgs = new PluginSettingEventArgs(((IPluginSetting)propGrid.SelectedObject).OwnerGuid,
//                                                        EventSource.User,
//                                                        propGrid.SelectedObject.GetType()
//                                                                .GetProperty(e.ChangedItem?.PropertyDescriptor?.Name ??
//                                                                             string.Empty),
//                                                        e.ChangedItem?.Value);
//             host.GetService<IEventBus>().Trigger(PluginEventId.Settings_OnSettingChanged, eventArgs);
//          };
//          var tlp = new TableLayoutPanel
//          {
//             Dock = DockStyle.Fill,
//             ColumnCount = 2,
//             RowCount = 2,
//             Margin = new(0),
//          };
//
//          var resetSingleButton = new Button
//          {
//             Text = "Reset Selected",
//             Dock = DockStyle.Fill,
//             Margin = new(1),
//          };
//
//          resetSingleButton.Click += (_, _) =>
//          {
//             // Reset the selected property to its default value
//             if (propGrid.SelectedGridItem?.PropertyDescriptor is { } descriptor)
//             {
//                var defaultValueAttribute =
//                   descriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
//                if (defaultValueAttribute != null)
//                {
//                   propGrid.SelectedObject.GetType()
//                           .GetProperty(descriptor.Name)
//                          ?.SetValue(propGrid.SelectedObject, defaultValueAttribute.Value);
//                   propGrid.Refresh();
//                }
//             }
//          };
//          propGrid.SelectedGridItemChanged += (_, _) =>
//          {
//             // If the DefaultValue attribute is present, enable the reset button
//             if (propGrid.SelectedGridItem?.PropertyDescriptor is { } descriptor)
//             {
//                var defaultValueAttribute =
//                   descriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
//                resetSingleButton.Enabled = defaultValueAttribute != null;
//             }
//          };
//
//          var resetAllButton = new Button
//          {
//             Text = "Reset All",
//             Dock = DockStyle.Fill,
//             Margin = new(1),
//          };
//
//          resetAllButton.Click += (_, _) =>
//          {
//             // Reset all properties to their default values
//             foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(propGrid.SelectedObject))
//                if (prop.Attributes[typeof(DefaultValueAttribute)] is DefaultValueAttribute defaultValueAttribute)
//                {
//                   propGrid.SelectedObject.GetType()
//                           .GetProperty(prop.Name)
//                          ?.SetValue(propGrid.SelectedObject, defaultValueAttribute.Value);
//                   propGrid.Refresh();
//                }
//          };
//
//          tlp.ColumnStyles.Add(new(SizeType.Percent, 50));
//          tlp.ColumnStyles.Add(new(SizeType.Percent, 50));
//          tlp.RowStyles.Add(new(SizeType.Percent, 100));
//          tlp.RowStyles.Add(new(SizeType.Absolute, 30));
//
//          tlp.Controls.Add(propGrid, 0, 0);
//          tlp.SetColumnSpan(propGrid, 2);
//          tlp.Controls.Add(resetSingleButton, 0, 1);
//          tlp.Controls.Add(resetAllButton, 1, 1);
//
//          SettingsTab.TabPages.Add(new TabPage
//          {
//             Text = host.GuidToName(plugin),
//             Tag = plugin,
//             Controls = { tlp },
//          });
//       }
//
//       SetCustomTabWidth();
//    }
//
//    private void SetCustomTabWidth()
//    {
//       var maxTextWidth = 0;
//       foreach (TabPage tabPage in SettingsTab.TabPages)
//       {
//          var sizeText = TextRenderer.MeasureText(tabPage.Text, SettingsTab.Font);
//          if (sizeText.Width > maxTextWidth)
//             maxTextWidth = sizeText.Width;
//       }
//
//       SettingsTab.ItemSize = new(28, Math.Max(maxTextWidth + 3, 30));
//    }
//
//    public void FocusOnTab(Guid pluginGuid)
//    {
//       if (pluginGuid == Guid.Empty)
//          return;
//
//       foreach (TabPage tabPage in SettingsTab.TabPages)
//          if (tabPage.Tag is Guid tag && tag.Equals(pluginGuid))
//          {
//             SettingsTab.SelectedTab = tabPage;
//             return;
//          }
//
//       // If no matching tab found, select the first tab.
//       if (SettingsTab.TabPages.Count > 0)
//          SettingsTab.SelectedIndex = 0;
//    }
// }

