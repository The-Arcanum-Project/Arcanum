using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.GameObjects.BaseTypes;
using Arcanum.UI.NUI.Generator;
using Common.Logger;

namespace Arcanum.UI.SpecializedEditors.Management;

/*
 * SPECIALIZED EDITORS
 *
 * Idea:
 * A specialized editor is a custom UI component designed to work on a specific object type or the NxProp of a specific type.
 *
 * Workflow:
 * For each object loaded into NUI we check if we have editors for its type or any of its properties.
 * Each found editor is then displayed in a tab on the right hand side.
 *
 * Specialized editor:
 * - Has a validation action that checks if it can work with the provided object or property.
 *   (e.g. value x is uniquely assigned to any object of that type)
 * - Name to display on the tab
 */

/// <summary>
/// Manages specialized editors for different object types and properties in the NUI.
/// Is kept as an instance to allow for multiple windows with different sets of specialized editors in the future.
/// This instance will be passed into the <see cref="Eu5UiGen"/> to be consumed there.
/// </summary>
public class SpecializedEditorsManager
{
   private readonly Dictionary<Type, ISpecializedEditor> _typeEditors = new();
   private readonly Dictionary<Type, ISpecializedEditor> _propertyEditors = new();

   // Dictionaries to hold already created editor instances for reuse
   private readonly Dictionary<Type, SpecializedEditor> _createdTypeEditors = new();
   private readonly Dictionary<Type, SpecializedEditor> _createdPropertyEditors = new();

   // Reused TabControl for displaying specialized editors
   private readonly TabControl _editorsTabControl = new();

   private readonly TextBlock _noEditorsTextBlock = new()
   {
      Text = "No specialized editors available for the selected object.",
      Margin = new(10),
      VerticalAlignment = VerticalAlignment.Center,
      HorizontalAlignment = HorizontalAlignment.Center,
      FontSize = 14,
      TextWrapping = TextWrapping.Wrap,
      TextAlignment = TextAlignment.Center,
   };

   private readonly TextBlock _noSelectionTextBlock = new()
   {
      Text = "No object selected.",
      Margin = new(10),
      VerticalAlignment = VerticalAlignment.Center,
      HorizontalAlignment = HorizontalAlignment.Center,
      FontSize = 14,
      TextWrapping = TextWrapping.Wrap,
      TextAlignment = TextAlignment.Center,
   };

   public void RegisterTypeEditor(Type targetType, ISpecializedEditor editor)
   {
      Debug.Assert(!_typeEditors.ContainsKey(targetType),
                   $"A specialized editor for type {targetType.FullName} is already registered.");
      _typeEditors[targetType] = editor;
   }

   public void RegisterPropertyEditor(Type propertyType, ISpecializedEditor editor)
   {
      Debug.Assert(!_propertyEditors.ContainsKey(propertyType),
                   $"A specialized editor for property type {propertyType.FullName} is already registered.");
      _propertyEditors[propertyType] = editor;
   }

   public bool TryGetTypeEditor(Type targetType, [MaybeNullWhen(false)] out ISpecializedEditor editor) => _typeEditors.TryGetValue(targetType, out editor);

   public bool TryGetPropertyEditor(Type propertyType, [MaybeNullWhen(false)] out ISpecializedEditor editor)
      => _propertyEditors.TryGetValue(propertyType, out editor);

   /// <summary>
   /// Null in the list of properties indicates that the editor is for the object type itself.
   /// </summary>
   /// <param name="targetObject"></param>
   /// <returns></returns>
   private List<KeyValuePair<ISpecializedEditor, List<Enum?>>> GetAllEditorsForType(IEu5Object targetObject)
   {
      List<KeyValuePair<ISpecializedEditor, List<Enum?>>> editors = [];

      // Get the editor for the object type itself
      if (TryGetTypeEditor(targetObject.GetType(), out var typeEditor))
         editors.Add(new(typeEditor, [null]));

      foreach (var prop in targetObject.GetAllProperties())
      {
         if (targetObject.IsCollection(prop))
         {
            var itemType = targetObject.GetNxItemType(prop)!;
            RegisterEditorForProperty(itemType, editors, prop);
         }
         else
         {
            var nxPropType = targetObject.GetNxPropType(prop);
            RegisterEditorForProperty(nxPropType, editors, prop);
         }
      }

      editors.Sort((a, b) => b.Key.Priority.CompareTo(a.Key.Priority));

      return editors;
   }

   private void RegisterEditorForProperty(Type nxPropType, List<KeyValuePair<ISpecializedEditor, List<Enum?>>> editors, Enum prop)
   {
      if (TryGetPropertyEditor(nxPropType, out var propEditor))
      {
         // Check if we already have this editor registered (can happen if multiple properties are of the same type)
         var existingEntry = editors.FirstOrDefault(e => e.Key == propEditor);
         if (existingEntry.Key != null)
            existingEntry.Value.Add(prop);
         else
            editors.Add(new(propEditor, [prop]));
      }
   }

   public FrameworkElement ConstructEditorViewForObject(List<IEu5Object> targets)
   {
      if (targets.Count == 0)
         return _noSelectionTextBlock;

      var editors = GetAllEditorsForType(targets[0]);
      if (editors.Count == 0)
         return _noEditorsTextBlock;
   
      // Check if current tabs are still valid for the new targets
      foreach (var (editor, props) in editors)
      {
         if (_editorsTabControl.Items.OfType<TabItem>().FirstOrDefault(ti => (string)ti.Header == editor.DisplayName) is not { } tabItem)
            continue;

         var existingEditorView = tabItem.Content as SpecializedEditor;
         if (existingEditorView == null)
            continue;

         // Check if the existing editor view can handle the new targets
         if (editor.SupportsMultipleTargets)
            existingEditorView.UpdateForNewTargets(props, targets);
         else
            existingEditorView.UpdateForNewTarget(props, targets[0]);
      }
      
      if (_editorsTabControl.Items.Count == editors.Count)
      {
         var allMatch = true;

         for (var i = 0; i < editors.Count; i++)
         {
            var (editor, _) = editors[i];

            if (_editorsTabControl.Items[i] is TabItem tabItem &&
                Equals(tabItem.Header, editor.DisplayName)) continue;
            
            allMatch = false;
            break;
         }

         if (allMatch)
            return _editorsTabControl;
      }

      _editorsTabControl.Items.Clear();
      foreach (var (editor, props) in editors)
      {
         var editorView = GetEditorView(editor, props, targets);
         var tabItem = new TabItem
         {
            Header = editor.DisplayName, Content = editorView,
         };
         _editorsTabControl.Items.Add(tabItem);
      }

      return _editorsTabControl;
   }

   private SpecializedEditor GetEditorView(ISpecializedEditor editor, List<Enum?> props, List<IEu5Object> targets)
   {
      SpecializedEditor? specializedEditor;
      // We have an IEu5Object type editor
      if (props is [null])
      {
         if (!_createdTypeEditors.TryGetValue(editor.GetType(), out specializedEditor))
         {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            specializedEditor = new(editor);
#if DEBUG
            sw.Stop();
            ArcLog.WriteLine("SEM",
                             LogLevel.INF,
                             "Created specialized editor of type {0} in {1} ms",
                             editor.GetType().Name,
                             sw.ElapsedMilliseconds);
#endif
            _createdTypeEditors[editor.GetType()] = specializedEditor;
         }
      }
      // We are targeting a property type editor
      else
      {
         if (!_createdPropertyEditors.TryGetValue(editor.GetType(), out specializedEditor))
         {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            specializedEditor = new(editor);
#if DEBUG
            sw.Stop();
            ArcLog.WriteLine("SEM",
                             LogLevel.INF,
                             "Created specialized editor of type {0} in {1} ms",
                             editor.GetType().Name,
                             sw.ElapsedMilliseconds);
#endif
            _createdPropertyEditors[editor.GetType()] = specializedEditor;
         }
      }

      if (editor.SupportsMultipleTargets)
         specializedEditor.UpdateForNewTargets(props, targets);
      else
         specializedEditor.UpdateForNewTarget(props, targets[0]);
      return specializedEditor;
   }
}