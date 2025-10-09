using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Jomini.Modifiers;
using Arcanum.Core.GameObjects.Common;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.Components.Windows.PopUp;
using Common.UI.MBox;

namespace Arcanum.UI.Components.Windows.MinorWindows;

public partial class ModifierCreator
{
   #region Dependency Properties

   public static readonly DependencyProperty ModifierNameProperty =
      DependencyProperty.Register(nameof(ModifierName),
                                  typeof(string),
                                  typeof(ModifierCreator),
                                  new(null, OnModifierNameChanged));

   public string ModifierName
   {
      get => (string)GetValue(ModifierNameProperty);
      set => SetValue(ModifierNameProperty, value);
   }

   public static readonly DependencyProperty ModifierLocalisationProperty =
      DependencyProperty.Register(nameof(ModifierLocalisation),
                                  typeof(string),
                                  typeof(ModifierCreator),
                                  new(default(string)));

   public string ModifierLocalisation
   {
      get => (string)GetValue(ModifierLocalisationProperty);
      set => SetValue(ModifierLocalisationProperty, value);
   }

   public static readonly DependencyProperty ModifierValueProperty =
      DependencyProperty.Register(nameof(ModifierValue),
                                  typeof(string),
                                  typeof(ModifierCreator),
                                  new(default(string)));

   public string ModifierValue
   {
      get => (string)GetValue(ModifierValueProperty);
      set => SetValue(ModifierValueProperty, value);
   }

   public static readonly DependencyProperty AllModifierDefinitionsProperty =
      DependencyProperty.Register(nameof(AllModifierDefinitions),
                                  typeof(List<string>),
                                  typeof(ModifierCreator),
                                  new(default(List<string>)));

   public List<string> AllModifierDefinitions
   {
      get => (List<string>)GetValue(AllModifierDefinitionsProperty);
      set => SetValue(AllModifierDefinitionsProperty, value);
   }

   public static readonly DependencyProperty ModifierDefaultValuesProperty =
      DependencyProperty.Register(nameof(ModifierDefaultValues),
                                  typeof(List<string>),
                                  typeof(ModifierCreator),
                                  new(default(List<string>)));

   public List<string> ModifierDefaultValues
   {
      get => (List<string>)GetValue(ModifierDefaultValuesProperty);
      set => SetValue(ModifierDefaultValuesProperty, value);
   }

   public ModValInstance? CreatedInstance { get; private set; }

   #endregion

   public ModifierCreator()
   {
      InitializeComponent();
      AllModifierDefinitions = Globals.ModifierDefinitions.Keys.ToList();
      ModifierNameBox.FullItemsSource = AllModifierDefinitions;
      ModifierName = AllModifierDefinitions.FirstOrDefault() ?? string.Empty;
   }

   private void CancelButton_Click(object sender, RoutedEventArgs e)
   {
      CreatedInstance = null;
      Close();
   }

   private void CreateButton_Click(object sender, RoutedEventArgs e)
   {
      if (string.IsNullOrWhiteSpace(ModifierName) || string.IsNullOrWhiteSpace(ModifierValue))
      {
         MBox.Show("Modifier Name and Value cannot be empty.", "Error", MBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      if (!Globals.ModifierDefinitions.TryGetValue(ModifierName, out var selectedModifier))
      {
         MBox.Show("Selected modifier definition not found.",
                   "Error",
                   MBoxButton.OK,
                   MessageBoxImage.Error);
         return;
      }

      CreatedInstance = new() { Definition = selectedModifier, Value = ModifierValue };
      DialogResult = true;
      Close();
   }

   private static void OnModifierNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var window = (ModifierCreator)d;
      var newModifierName = (string)e.NewValue;

      if (string.IsNullOrWhiteSpace(newModifierName))
      {
         window.ModifierLocalisation = string.Empty;
         window.ModifierDefaultValues = [];
         return;
      }

      if (Globals.ModifierDefinitions.TryGetValue(newModifierName, out var selectedModifier))
      {
         window.ModifierLocalisation = selectedModifier.UniqueId;
         window.ModifierDefaultValues = ModifierManager.GetDefaultValuesForModifier(selectedModifier);
      }
      else
      {
         window.ModifierLocalisation = "Unknown Modifier";
         window.ModifierDefaultValues = [];
      }
   }

   private void ModifierNameBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (e.AddedItems.Count > 0 && e.AddedItems[0] is string selectedName && selectedName != ModifierName)
      {
         ModifierName = selectedName;
         ModifierValueBox.EditableTextBox.Clear();
         ModifierDefaultValues = ModifierManager.GetDefaultValuesForModifier(Globals.ModifierDefinitions[selectedName]);
      }
   }
}