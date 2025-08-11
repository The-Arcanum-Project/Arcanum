using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.StyleClasses;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class GameObjectBrowser
{
   public ObservableCollection<PropertyInfo> PropertyInfos { get; set; }
   public ObservableCollection<FieldInfo> FieldInfos { get; set; }

   public GameObjectBrowser()
   {
      InitializeComponent();

      DataContext = this;

      var type = typeof(Globals);
      PropertyInfos = new(type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
      FieldInfos = new(type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
   }

   private void OnPropertyOpenClick(object sender, RoutedEventArgs e)
   {
      if (sender is BaseButton { Tag: PropertyInfo propertyInfo })
      {
         var value = propertyInfo.GetValue(null);
         AppData.WindowLinker.GetPropertyGridOrCollectionView(value).ShowDialog();
         GC.Collect(); // Force garbage collection to clean up any unused objects
      }
   }
}