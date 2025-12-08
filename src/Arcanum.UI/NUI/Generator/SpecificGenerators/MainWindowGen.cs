using System.Diagnostics;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.LocationCollections;
using Arcanum.Core.GameObjects.Pops;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.UserControls.ValueAllocators;
using Arcanum.UI.SpecializedEditors.Editors;
using Arcanum.UI.SpecializedEditors.Management;

namespace Arcanum.UI.NUI.Generator.SpecificGenerators;

public static class MainWindowGen
{
   private static readonly SpecializedEditorsManager SpecialEditorMngr = new();
   private static ContentPresenter _specialEditorsHost = null!;
   private static bool _isInitialized;

   public static void Initialize(ContentPresenter specialEditorsHost)
   {
      Debug.Assert(!_isInitialized, "MainWindowGen is already initialized.");
      // TODO: Initialize all the Editors to the manager
      SpecialEditorMngr.RegisterTypeEditor(typeof(Province), new ProvinceEditor());
      SpecialEditorMngr.RegisterPropertyEditor(typeof(PopDefinition), new PopsEditor());

      _specialEditorsHost = specialEditorsHost;
      _isInitialized = true;
   }

   /// <summary>
   /// Sets the Main Editing Panel of Arcanum Main Window.
   /// <br/>
   /// Also sets the Specialized Editors if there are any
   /// </summary>
   public static void GenerateAndSetView(NavH navh, List<Enum>? markedProps = null!, bool hasHeader = true)
   {
      Debug.Assert(_isInitialized, "MainWindowGen is not initialized. Call Initialize() first.");

      if (navh.Targets.Count > 0)
      {
         var empty = EmptyRegistry.Empties[navh.Targets[0].GetType()];
         if (navh.Targets.Any(t => t.Equals(empty)))
         {
            // We do not want to show empties
            navh.Root.Content = null;
            return;
         }
      }

      //TODO: @Melco temporary fix for preview not clearing properly
      SelectionManager.ClearPreview();
      navh.Root.Content = Eu5UiGen.GenerateView(navh, markedProps ?? [], hasHeader);

      SetSpecializedEditors(navh);
   }

   private static void SetSpecializedEditors(NavH navh)
   {
      var content = SpecialEditorMngr.ConstructEditorViewForObject(navh.Targets);
      _specialEditorsHost.Content = content;
      if (content is TabControl tc)
         tc.SelectedIndex = 0;
   }

   public static void UpdateSpecializedEditors()
   {
      var currNavh = NUINavigation.Instance.Current;
      if (currNavh != null)
         SetSpecializedEditors(currNavh);
   }
}