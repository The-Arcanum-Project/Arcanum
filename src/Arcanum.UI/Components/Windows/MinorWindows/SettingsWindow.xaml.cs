#define DEBUG_OBJ

using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Arcanum.API.Attributes;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Settings;
using Arcanum.UI.Components.UserControls.BaseControls;
using Arcanum.UI.Util.WindowManagement;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SettingsWindow
{
   private static readonly UIElement LoadingPlaceholder = new TextBlock
   {
      Text = "Loading...",
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
   };

   public static string LastSelectedProperty { get; set; } = string.Empty;

   public SettingsWindow()
   {
      InitializeComponent();
   }

   public static SettingsWindow ShowSettingsWindow()
   {
      var settingsWindow = new SettingsWindow();

      settingsWindow.SettingsTabControl.SelectionChanged += settingsWindow.TabControl_SelectionChanged;
      CreateTabsLazily(settingsWindow.SettingsTabControl, Config.Settings);

      WindowManager.OpenWindow(settingsWindow);
      if (!string.IsNullOrEmpty(LastSelectedProperty))
         settingsWindow.NavigateToSetting(LastSelectedProperty.Split('/'));
      return settingsWindow;
   }

   /// <summary>
   /// Creates the tab structure without generating the expensive content.
   /// Content is loaded on-demand when a tab is selected.
   /// </summary>
   private static void CreateTabsLazily(TabControl tc, object obj)
   {
      var settingsProperties = GetPublicProperties(obj);
      settingsProperties.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

      foreach (var property in settingsProperties)
      {
         var subMenuName = GetSubMenuName(property);
         var tabItem = new TabItem
         {
            Header = subMenuName ?? property.Name,
            Content = LoadingPlaceholder,
            Tag = (property, obj),
         };
         tc.Items.Add(tabItem);
      }
   }

   /// <summary>
   /// Event handler that triggers the generation of tab content when a tab is selected for the first time.
   /// </summary>
   private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (e.AddedItems.Count == 0 || e.AddedItems[0] is not TabItem selectedTab)
         return;

      LoadTabContentIfNeeded(selectedTab);
   }

   /// <summary>
   /// Checks if a TabItem's content needs to be loaded and generates it if necessary.
   /// </summary>
   private void LoadTabContentIfNeeded(TabItem tabItem)
   {
      if (tabItem.Tag is not (PropertyInfo property, { } parentObject))
         return;

      tabItem.Content = GenerateContentForProperty(property, parentObject);
      tabItem.Tag = null;
   }

   /// <summary>
   /// Contains the original logic to generate either a PropertyGrid or a nested TabControl for a given property.
   /// </summary>
   private UIElement GenerateContentForProperty(PropertyInfo property, object parentObject)
   {
      var subMenuName = GetSubMenuName(property);
      var isSubItem = subMenuName != null;
      var propertyValue = property.GetValue(parentObject)!;

      if (isSubItem)
         return GenerateSubMenu(propertyValue);

      if (PropertyGrid.InformIfEditorAvailable(parentObject))
         return new PropertyGrid
         {
            LabelWidth = subMenuName != null ? 100 : 250,
            SelectedObject = propertyValue,
            Name = property.Name,
            Margin = new(0),
            Padding = new(0),
            ForceInlinePropertyGrid = property.GetCustomAttribute<SettingsForceInlinePropertyGrid>() != null,
         };

      return new TextBlock { Text = "No editor available for this property." };
   }

   private TabControl GenerateSubMenu(object subMenuObject)
   {
      TabControl tc = new()
      {
         Margin = new(0),
         Padding = new(0),
         TabStripPlacement = Dock.Left,
      };

      tc.SelectionChanged += TabControl_SelectionChanged;

      CreateTabsLazily(tc, subMenuObject);
      return tc;
   }

   private void InitTabs(TabControl tc, object obj)
   {
      var settingsProperties = GetPublicProperties(obj);
      settingsProperties.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

      foreach (var property in settingsProperties)
      {
         var subMenuName = GetSubMenuName(property);
         var isSubItem = subMenuName != null;
         var tabItem = new TabItem { Header = subMenuName ?? property.Name };

         if (isSubItem)
            tabItem.Content = GenerateSubMenu(property.GetValue(obj)!);
         else if (PropertyGrid.InformIfEditorAvailable(obj))
            tabItem.Content = new PropertyGrid
            {
               LabelWidth = subMenuName != null ? 100 : 250,
               SelectedObject = property.GetValue(obj),
               Name = property.Name,
               Margin = new(0),
               Padding = new(0),
               ForceInlinePropertyGrid =
                  property.GetCustomAttribute<SettingsForceInlinePropertyGrid>() != null,
            };

         tc.Items.Add(tabItem);
      }
   }

   public void NavigateToSetting(string[] path)
   {
      var tabControl = SettingsTabControl;
      var i = 0;

      while (i < path.Length)
      {
         var step = path[i];

         var tabs = tabControl.Items.OfType<TabItem>();
         foreach (var item in tabs)
            if (item.Header.ToString()?.Equals(step) ?? false)
            {
               tabControl.SelectedItem = item;
               LoadTabContentIfNeeded(item);

               if (item.Content is TabControl nestedTabControl)
                  tabControl = nestedTabControl;

               if (i < path.Length - 1 && item.Content is PropertyGrid pg && pg.NavigateToProperty(path[i + 1]))
                  return; // We found the property grid, no need to continue

               break;
            }

         i++;
      }
   }

   private static List<PropertyInfo> GetPublicProperties(object obj)
   {
      return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsClass &&
                            p.PropertyType != typeof(string) &&
                            p.PropertyType != typeof(object) &&
                            p.GetCustomAttribute<IgnoreInPropertyGrid>() is null)
                .ToList();
   }

   private static string? GetSubMenuName(PropertyInfo property)
   {
      var attribute = property.GetCustomAttribute<IsSubMenuAttribute>();
      return attribute?.Name;
   }

   /// <summary>
   /// Any property that has a <see cref="CustomResetMethod"/> attribute will be reset using the specified method.
   /// The method must have the signature: <c>object Method(PropertyInfo)</c>.
   /// </summary>
   /// <param name="propGrid"></param>
   /// <param name="info"></param>
   /// <exception cref="InvalidOperationException"></exception>
   private static void ResetPropertyToDefault(PropertyGrid propGrid, PropertyInfo info)
   {
      var customMethod = info.GetCustomAttribute<CustomResetMethod>();
      object? newValue;

      if (customMethod is not null)
      {
         var methodInfo = info.DeclaringType?.GetMethod(customMethod.MethodName,
                                                        BindingFlags.Public |
                                                        BindingFlags.NonPublic |
                                                        BindingFlags.Instance);
         if (methodInfo is null)
            return;

         var parameters = methodInfo.GetParameters();
         if (methodInfo.ReturnType != typeof(object) ||
             parameters.Length != 1 ||
             parameters[0].ParameterType != typeof(PropertyInfo))
            throw new
               InvalidOperationException($"Method '{customMethod.MethodName}' must have signature: object Method(PropertyInfo)");

         newValue = methodInfo.Invoke(propGrid.SelectedObject, parameters: [info]);
      }
      else
      {
         var defaultValueAttribute = info.GetCustomAttribute<DefaultValueAttribute>();
         if (defaultValueAttribute == null)
            return;

         newValue = defaultValueAttribute.Value;
      }

      info.SetValue(propGrid.SelectedObject, newValue);
      propGrid.UpdatePropertyItem();
   }

   private static PropertyGrid? FindActivePropertyGrid(TabControl rootTabControl)
   {
      while (true)
         switch (rootTabControl.SelectedContent)
         {
            case PropertyGrid pg:
               return pg.HasInlinedPropertyGrid ? pg.InlinedPropertyGrid : pg;
            case TabControl nestedTab:
               rootTabControl = nestedTab;
               continue;
            case TabItem tp:
               return FindActivePropertyGridFromTabItem(tp.Content);
            default:
               return null;
         }
   }

   private static PropertyGrid? FindActivePropertyGridFromTabItem(object? content)
   {
      return content switch
      {
         PropertyGrid pg => pg,
         TabControl tc => FindActivePropertyGrid(tc),
         _ => null,
      };
   }

   private void ResetAllSettings_OnClick(object sender, RoutedEventArgs e)
   {
      var result = MessageBox.Show("Are you sure you want to reset all settings to default?",
                                   "Reset AgsSettings",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning);
      if (result != MessageBoxResult.Yes)
         return;

      Config.Settings = new();
      SettingsTabControl.Items.Clear();
      InitTabs(SettingsTabControl, Config.Settings);
   }

   private void ResetSelectedTabItem_OnClick(object sender, RoutedEventArgs e)
   {
      var propGrid = FindActivePropertyGrid(SettingsTabControl);
      if (propGrid is null)
         return;

      var selectedObject = propGrid.SelectedObject;
      if (selectedObject == null)
         return;

      var customResetMethod = selectedObject.GetType().GetCustomAttribute<CustomResetMethod>();
      if (customResetMethod == null)
      {
         // We need to trigger a change event for all properties in the object to propagate the changes to other UI elements.
         var propsToReset = selectedObject.GetType()
                                          .GetProperties(BindingFlags.Public | BindingFlags.Instance);
         foreach (var property in propsToReset)
            ResetPropertyToDefault(propGrid, property);
      }
      else
      {
         var methodInfo = selectedObject.GetType()
                                        .GetMethod(customResetMethod.MethodName,
                                                   BindingFlags.Public | BindingFlags.Instance);

         if (methodInfo == null ||
             methodInfo.ReturnType != selectedObject.GetType() ||
             methodInfo.GetParameters().Length != 0)
            throw new
               InvalidOperationException($"Method '{customResetMethod.MethodName}' must have signature: {selectedObject.GetType().Name} Method()");

         propGrid.SelectedObject = methodInfo.Invoke(selectedObject, null);
      }
   }

   private void ResetSelected_OnClick(object sender, RoutedEventArgs e)
   {
      var propGrid = FindActivePropertyGrid(SettingsTabControl);
      if (propGrid == null)
         return;

      var selectedProperty = propGrid.SelectedPropertyItem?.PropertyInfo;
      if (selectedProperty == null)
         return;

      ResetPropertyToDefault(propGrid.GetActive(), selectedProperty);
   }

   private void Window_Closed(object sender, EventArgs e)
   {
      JsonProcessor.Serialize(Path.Combine(IO.GetArcanumDataPath, Config.CONFIG_FILE_NAME), Config.Settings);
      LastSelectedProperty = GetCurrentSettingPath(SettingsTabControl);
   }

   private static string GetCurrentSettingPath(TabControl rootTabControl)
   {
      var tabPath = GetCurrentTabPath(rootTabControl);
      var propertyPath = GetCurrentPropertyPath(rootTabControl);
      return string.Join("/", tabPath.Concat(propertyPath));
   }

   private static List<string> GetCurrentTabPath(TabControl rootTabControl)
   {
      var path = new List<string>();
      var currentTabControl = rootTabControl;

      while (true)
      {
         if (currentTabControl.SelectedItem is not TabItem selectedTab)
            break;

         path.Add(selectedTab.Header.ToString() ?? string.Empty);

         if (selectedTab.Content is TabControl nestedTabControl)
         {
            currentTabControl = nestedTabControl;
            continue;
         }

         break;
      }

      return path;
   }

   private static List<string> GetCurrentPropertyPath(TabControl rootTabControl)
   {
      var path = new List<string>();

      while (true)
      {
         var selected = rootTabControl.SelectedContent;

         if (selected is PropertyGrid pg)
         {
            if (pg.HasInlinedPropertyGrid)
               path.AddRange(GetCurrentPropertyPathFromPropertyGrid(pg.InlinedPropertyGrid));
            else if (pg.SelectedPropertyItem != null)
               path.Add(pg.SelectedPropertyItem.PropertyInfo.Name);

            return path;
         }

         if (selected is TabControl nestedTab)
         {
            rootTabControl = nestedTab;
            continue;
         }

         if (selected is TabItem tp)
         {
            path.Add(tp.Header.ToString() ?? string.Empty);
            var nestedPath = GetCurrentPropertyPathFromTabItem(tp.Content);
            path.AddRange(nestedPath);
         }

         return path;
      }
   }

   private static List<string> GetCurrentPropertyPathFromTabItem(object? content)
   {
      return content switch
      {
         PropertyGrid pg => GetCurrentPropertyPathFromPropertyGrid(pg),
         TabControl tc => GetCurrentPropertyPath(tc),
         _ => [],
      };
   }

   private static List<string> GetCurrentPropertyPathFromPropertyGrid(PropertyGrid? pg)
   {
      if (pg == null)
         return [];

      var path = new List<string>();
      if (pg.SelectedPropertyItem != null)
         path.Add(pg.SelectedPropertyItem.PropertyInfo.Name);

      if (pg.HasInlinedPropertyGrid)
         path.AddRange(GetCurrentPropertyPathFromPropertyGrid(pg.InlinedPropertyGrid));

      return path;
   }
}