#define DEBUG_OBJ

using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
      settingsWindow.InitTabs(settingsWindow.SettingsTabControl, Globals.Settings);
      settingsWindow.Show();
      return settingsWindow;
   }

#if DEBUG_OBJ
   // ReSharper disable once FieldCanBeMadeReadOnly.Local
   private AllOptionsTestObject _allOptionsTestObject = new();
#endif

   private void InitTabs(TabControl tc, object obj)
   {
#if DEBUG_OBJ
      _allOptionsTestObject = new ();
      var item = new TabItem
      {
         Header = "PropGirdDebug",
         Content = new PropertyGrid
         {
            SelectedObject = _allOptionsTestObject, Name = _allOptionsTestObject.GetType().Name,
         },
      };
      tc.Items.Add(item);
#endif

      var settingsProperties = GetPublicProperties(obj);

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
                            SelectedObject = property.GetValue(obj),
                            Name = property.Name,
                            Margin = new(0),
                            Padding = new(0),
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

   private static List<PropertyInfo> GetPublicProperties(object obj)
   {
      return obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsClass &&
                            p.PropertyType != typeof(string) &&
                            p.PropertyType != typeof(object))
                .ToList();
   }

   private static string? GetSubMenuName(PropertyInfo property)
   {
      var attribute = property.GetCustomAttribute<IsSubMenuAttribute>();
      return attribute?.Name;
   }

   private static void ResetPropertyToDefault(PropertyGrid propGrid, PropertyInfo info)
   {
      var defaultValueAttribute = info.GetCustomAttribute<DefaultValueAttribute>();
      if (defaultValueAttribute == null)
         return;

      info.SetValue(propGrid.SelectedObject, defaultValueAttribute.Value);
      propGrid.UpdatePropertyItem();
   }

   private static PropertyGrid? FindActivePropertyGrid(TabControl rootTabControl)
   {
      while (true)
      {
         var selected = rootTabControl.SelectedContent;

         if (selected is PropertyGrid pg)
            return pg;

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
                                   "Reset Settings",
                                   MessageBoxButton.YesNo,
                                   MessageBoxImage.Warning);
      if (result != MessageBoxResult.Yes)
         return;

      Globals.Settings = new();
      SettingsTabControl.Items.Clear();
      InitTabs(SettingsTabControl, Globals.Settings);
   }

   private void ResetSelectedTabItem_OnClick(object sender, RoutedEventArgs e)
   {
      var propGrid = FindActivePropertyGrid(SettingsTabControl);
      if (propGrid is null)
         return;

      var selectedObject = propGrid.SelectedObject;
      if (selectedObject == null)
         return;

      propGrid.SelectedObject = Activator.CreateInstance(selectedObject.GetType(), true);
   }

   private void ResetSelected_OnClick(object sender, RoutedEventArgs e)
   {
      var propGrid = FindActivePropertyGrid(SettingsTabControl);
      if (propGrid == null)
         return;

      var selectedProperty = propGrid.SelectedPropertyItem?.PropertyInfo;
      if (selectedProperty == null)
         return;

      ResetPropertyToDefault(propGrid, selectedProperty);
   }
}