using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GlobalStates;
using Timer = System.Timers.Timer;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ModifierBrowser
{
   private readonly Timer _searchTimer = new(250);

   public ModifierBrowser()
   {
      InitializeComponent();
      var exampleDocObj = new ModifierDefinition();
      var docObjPublicProperties = exampleDocObj.GetType().GetProperties().Where(p => p.GetGetMethod() != null);
      foreach (var property in docObjPublicProperties)
         PropertyFilterComboBox.Items.Add(property.Name);

      Loaded += OnLoaded;
      _searchTimer.Elapsed += (_, _) =>
      {
         _searchTimer.Stop();
         Dispatcher.Invoke(() => Search(SearchTextBox.Text));
      };
   }

   public static ModifierBrowser ShowModifierBrowser()
   {
      var browser = new ModifierBrowser
      {
         DocsObjDataGrid = { DataContext = new ListCollectionView(Globals.ModifierDefinitions.Values.ToList()) }
      };

      return browser;
   }

   private void OnLoaded(object o, RoutedEventArgs routedEventArgs)
   {
      PropertyFilterComboBox.SelectedIndex = 0;
      Keyboard.Focus(SearchTextBox);
   }

   private void Search(string query)
   {
      var propertyInfo = GetSelectedProperty();
      if (propertyInfo == null!)
         return;

      if (DocsObjDataGrid.DataContext is not ListCollectionView view)
         throw new InvalidOperationException("DataContext is not a CollectionView.");

      view.Filter = item =>
      {
         if (item is not ModifierDefinition docObj)
            return false;

         // if the value is an array search through each element
         if (propertyInfo.PropertyType.IsArray)
         {
            if (propertyInfo.GetValue(docObj) is not Array array)
               return false;

            return array.Cast<string>()
                        .Any(element => element?.Contains(query, StringComparison.OrdinalIgnoreCase) ==
                                        true);
         }

         var propertyValue = propertyInfo.GetValue(docObj)?.ToString() ?? string.Empty;
         return string.IsNullOrEmpty(query) ||
                propertyValue.Contains(query, StringComparison.OrdinalIgnoreCase);
      };

      view.Refresh();
   }

   private PropertyInfo GetSelectedProperty()
   {
      if (PropertyFilterComboBox.SelectedItem is not string selectedProperty)
         return null!;

      var exampleDocObj = new ModifierDefinition();
      var propertyInfo = exampleDocObj.GetType().GetProperty(selectedProperty);
      if (propertyInfo == null)
         throw new InvalidOperationException($"Property '{selectedProperty}' not found on DocsObj.");

      return propertyInfo;
   }

   private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
   {
      _searchTimer.Stop();
      _searchTimer.Start();
   }

   private void PropertyFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      Search(SearchTextBox.Text);
   }
}