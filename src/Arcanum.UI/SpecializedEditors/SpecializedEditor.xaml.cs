using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.NUI.Generator.SpecificGenerators;
using Arcanum.UI.SpecializedEditors.Management;
using CommunityToolkit.Mvvm.Input;

namespace Arcanum.UI.SpecializedEditors;

public partial class SpecializedEditor
{
   public static readonly DependencyProperty EditorContentProperty =
      DependencyProperty.Register(nameof(EditorContent),
                                  typeof(FrameworkElement),
                                  typeof(SpecializedEditor),
                                  new (default(FrameworkElement)));

   public static readonly DependencyProperty TargetablePropertiesProperty =
      DependencyProperty.Register(nameof(TargetableProperties),
                                  typeof(string[]),
                                  typeof(SpecializedEditor),
                                  new (default(string[])));

   public static readonly DependencyProperty RequirementsStatusTextProperty =
      DependencyProperty.Register(nameof(RequirementsStatusText),
                                  typeof(string),
                                  typeof(SpecializedEditor),
                                  new ("Available"));

   public string[] TargetableProperties
   {
      get => (string[])GetValue(TargetablePropertiesProperty);
      set => SetValue(TargetablePropertiesProperty, value);
   }

   public string RequirementsStatusText
   {
      get => (string)GetValue(RequirementsStatusTextProperty);
      set => SetValue(RequirementsStatusTextProperty, value);
   }
   public FrameworkElement? EditorContent
   {
      get => (FrameworkElement)GetValue(EditorContentProperty);
      set => SetValue(EditorContentProperty, value);
   }
   public RelayCommand CheckRequirementsCommand => new (UpdateRequirementsStatus);

   private readonly ISpecializedEditor _specializedEditor = null!;
   private object[] _targets = null!;
   private Enum?[] _targetProperty = null!;
   private bool _ignoreEnableChange = false;

   public SpecializedEditor()
   {
      InitializeComponent();
   }

   public SpecializedEditor(ISpecializedEditor spe) : this()
   {
      _specializedEditor = spe;
   }

   private void UpdateRequirementsStatus()
   {
      var index = PropertySelector.SelectedIndex;
      index = Math.Min(index, TargetableProperties.Length - 1);
      index = Math.Max(index, 0);

      var canEdit = _specializedEditor.CanEdit(_targets, _targetProperty[index]);

      RequirementsStatusText = canEdit ? "Available" : "Requirements not met.";
      if (canEdit)
         RequirementsStatusTextBlock.Foreground = Brushes.Green;
      else
         RequirementsStatusTextBlock.Foreground = Brushes.Red;
      EditorContent?.IsEnabled = canEdit && _specializedEditor.Enabled;
   }

   /// <summary>
   /// As we want to cache the specialized editor instances, we need to reset them for each new target.
   /// </summary>
   public void UpdateForNewTarget(List<Enum?> props, IEu5Object target)
   {
      _ignoreEnableChange = true;
      EnabledToggleButton.IsChecked = _specializedEditor.Enabled;
      _ignoreEnableChange = false;

      if (!_specializedEditor.Enabled)
      {
         EditorContent = null;
         return;
      }

      Debug.Assert(_specializedEditor != null, "Specialized editor instance must be provided.");
      Debug.Assert(props.Count > 0, "At least one targetable property must be provided.");
      Debug.Assert(!_specializedEditor.SupportsMultipleTargets,
                   "This method should not be used for editors that support multiple targets.");

      _targets = [target];
      _targetProperty = props.ToArray();
      TargetableProperties = props.Select(p => p?.ToString() ?? _targets[0].GetType().Name).ToArray();
      _specializedEditor.ResetFor(_targets);
      UpdateRequirementsStatus();
      SetEditorContent();
      PropertySelector.SelectedIndex = 0;
   }

   public void UpdateForNewTargets(List<Enum?> props, List<IEu5Object> targets)
   {
      EnabledToggleButton.IsChecked = _specializedEditor.Enabled;
      Debug.Assert(_specializedEditor != null, "Specialized editor instance must be provided.");
      Debug.Assert(props.Count > 0, "At least one targetable property must be provided.");
      Debug.Assert(_specializedEditor.SupportsMultipleTargets,
                   "This method should only be used for editors that support multiple targets.");

      _targets = targets.Cast<object>().ToArray();
      _targetProperty = props.ToArray();
      TargetableProperties = props.Select(p => p?.ToString() ?? _targets[0].GetType().Name).ToArray();
      _specializedEditor.ResetFor(_targets);
      UpdateRequirementsStatus();
      SetEditorContent();
      PropertySelector.SelectedIndex = 0;
   }

   private void SetEditorContent()
   {
      var newContent = _specializedEditor.GetEditorControl();
      
      // Force re-evaluation of the binding in case we have some new object with the same control instance.
      if (EditorContent == newContent) EditorContent = null;

      EditorContent = newContent;
   }

   private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is not ComboBox)
         return;

      UpdateRequirementsStatus();
   }

   private void ToggleButton_Update(object sender, RoutedEventArgs e)
   {
      if (_ignoreEnableChange || sender is not ToggleButton tb)
         return;

      _specializedEditor.Enabled = tb.IsChecked == true;
      MainWindowGen.UpdateSpecializedEditors();
   }
}