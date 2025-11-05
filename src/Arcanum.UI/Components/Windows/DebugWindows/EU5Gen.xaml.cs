using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.Core.Registry;
using Arcanum.UI.NUI.Generator;
using Arcanum.UI.NUI.Nui2.Nui2Gen;

namespace Arcanum.UI.Components.Windows.DebugWindows;

public partial class Eu5Gen
{
   public List<Type> NUIObjectTypes
   {
      get => (List<Type>)GetValue(NUIObjectTypesProperty);
      set => SetValue(NUIObjectTypesProperty, value);
   }

   public static readonly DependencyProperty NUIObjectTypesProperty =
      DependencyProperty.Register(nameof(NUIObjectTypes),
                                  typeof(List<Type>),
                                  typeof(Eu5Gen),
                                  new(new List<Type>()));

   public string SelectedObjectName
   {
      get => (string)GetValue(SelectedObjectNameProperty);
      set => SetValue(SelectedObjectNameProperty, value);
   }

   public static readonly DependencyProperty SelectedObjectNameProperty =
      DependencyProperty.Register(nameof(SelectedObjectName),
                                  typeof(string),
                                  typeof(Eu5Gen),
                                  new(default(string)));

   public ObservableCollection<IEu5Object> NUIObjects
   {
      get => (ObservableCollection<IEu5Object>)GetValue(NUIObjectsProperty);
      set => SetValue(NUIObjectsProperty, value);
   }

   public static readonly DependencyProperty NUIObjectsProperty =
      DependencyProperty.Register(nameof(NUIObjects),
                                  typeof(ObservableCollection<IEu5Object>),
                                  typeof(Eu5Gen),
                                  new(new ObservableCollection<IEu5Object>()));

   public Eu5Gen()
   {
      InitializeComponent();

      var types = Eu5ObjectsRegistry.Eu5Objects.ToList();
      types.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
      NUIObjectTypes = types;
   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView { SelectedItem: Type type })
         return;

      var dict = ((IEu5Object)EmptyRegistry.Empties[type]).GetGlobalItemsNonGeneric();

      var values = dict.Values.Cast<IEu5Object>().ToList();

      values.Sort((x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));

      NUIObjects = new(values);

      ObjectListView.SelectedIndex = 0;
   }

   private void ObjectSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ListView lv)
         return;

      var selectedItems = lv.SelectedItems.Cast<IEu5Object>().ToList();
      if (selectedItems.Count == 0)
      {
         SelectedObjectName = "No object selected";
         ViewPresenter.Content = null;
         return;
      }

      if (selectedItems.Count == 1)
         SelectedObjectName = selectedItems[0].ToString() ?? selectedItems[0].GetType().Name;
      else
         SelectedObjectName = $"{selectedItems.Count} items selected";

      Eu5UiGen.GenerateAndSetView(new(selectedItems, true, ViewPresenter, true));

      //NUIViewGenerator.GenerateAndSetView(new(selectedItems, true, ViewPresenter));
   }
}