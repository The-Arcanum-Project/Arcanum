#define DEBUG_OBJ

using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Arcanum.API.Attributes;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.Settings;
using Arcanum.UI.Components.UserControls.BaseControls;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class SettingsWindow
{
   public SettingsWindow()
   {
      InitializeComponent();
   }

   public static SettingsWindow ShowSettingsWindow()
   {
      var settingsWindow = new SettingsWindow();
      settingsWindow.InitTabs(settingsWindow.SettingsTabControl, Config.Settings);
      settingsWindow.Show();
      return settingsWindow;
   }

   private void InitTabs(TabControl tc, object obj)
   {
      var settingsProperties = GetPublicProperties(obj);
      settingsProperties.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

      foreach (var property in settingsProperties)
      {
         var subMenuName = GetSubMenuName(property);
         var isSubItem = subMenuName != null;
         var tabItem = new TabItem
         {
            Header = subMenuName ?? property.Name,
            Content = isSubItem
                         ? GenerateSubMenu(property.GetValue(obj)!)
                         : new PropertyGrid
                         {
                            LabelWidth = subMenuName != null ? 100 : 250,
                            SelectedObject = property.GetValue(obj),
                            Name = property.Name,
                            Margin = new(0),
                            Padding = new(0),
                            ForceInlinePropertyGrid =
                               property.GetCustomAttribute<SettingsForceInlinePropertyGrid>() != null,
                         },
         };
         tc.Items.Add(tabItem);
      }
   }

   private TabControl GenerateSubMenu(object subMenuObject)
   {
      TabControl tc = new()
      {
         Margin = new(0),
         Padding = new(0),
         TabStripPlacement = Dock.Left,
      };

      InitTabs(tc, subMenuObject);
      return tc;
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
      {
         var selected = rootTabControl.SelectedContent;

         if (selected is PropertyGrid pg)
         {
            if (pg.HasInlinedPropertyGrid)
               return pg.InlinedPropertyGrid;

            return pg;
         }

         if (selected is TabControl nestedTab)
         {
            rootTabControl = nestedTab;
            continue;
         }

         if (selected is TabItem tp)
            return FindActivePropertyGridFromTabItem(tp.Content);

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
         propGrid.SelectedObject = Activator.CreateInstance(selectedObject.GetType(), true);
      else
      {
         // Find the method in the selected object's type
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
}