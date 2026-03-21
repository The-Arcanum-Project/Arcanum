using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Arcanum.Core.CoreSystems.Selection;
using Arcanum.Core.GameObjects.InGame.Map.LocationCollections;
using Arcanum.Core.Registry;
using Arcanum.UI.Components.UserControls.ValueAllocators;
using Arcanum.UI.SpecializedEditors.Editors;
using Arcanum.UI.SpecializedEditors.Management;
using PopDefinition = Arcanum.Core.GameObjects.InGame.Pops.PopDefinition;

namespace Arcanum.UI.NUI.Generator.SpecificGenerators;

public static class MainWindowGen
{
   private static readonly SpecializedEditorsManager SpecialEditorMngr = new();
   private static ContentPresenter _specialEditorsHost = null!;
   private static bool _isInitialized;

   public static void Initialize(ContentPresenter specialEditorsHost)
   {
      Debug.Assert(!_isInitialized, "MainWindowGen is already initialized.");
      SpecialEditorMngr.EditorsTabControl.Style = (Style)Application.Current.FindResource("CenteredTabControlStyle")!;
      // TODO: Initialize all the Editors to the manager
      SpecialEditorMngr.RegisterTypeEditor(typeof(Province), new LocationCollectionSpecializedEditor<Location, Province>(Province.Field.Locations));
      SpecialEditorMngr.RegisterTypeEditor(typeof(Area), new LocationCollectionSpecializedEditor<Province, Area>(Area.Field.Provinces));
      SpecialEditorMngr.RegisterTypeEditor(typeof(Region), new LocationCollectionSpecializedEditor<Area, Region>(Region.Field.Areas));
      SpecialEditorMngr.RegisterTypeEditor(typeof(SubContinent), new LocationCollectionSpecializedEditor<Region, SubContinent>(SubContinent.Field.Regions));
      SpecialEditorMngr.RegisterTypeEditor(typeof(Continent), new LocationCollectionSpecializedEditor<SubContinent, Continent>(Continent.Field.SuperRegions));
      SpecialEditorMngr.RegisterTypeEditor(typeof(Country), new PoliticalEditor());

      SpecialEditorMngr.RegisterTypeEditor(typeof(Location), new InstitutionEditor());
      SpecialEditorMngr.RegisterPropertyEditor(typeof(PopDefinition), new PopsEditor());

      _specialEditorsHost = specialEditorsHost;
      _isInitialized = true;
   }

   /// <summary>
   ///    Sets the Main Editing Panel of Arcanum Main Window.
   ///    <br />
   ///    Also sets the Specialized Editors if there are any
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
      var selectedIndex = SpecialEditorMngr.EditorsTabControl.SelectedIndex;
      var tabHash = SpecialEditorMngr.EditorsTabControl.Items.Cast<TabItem>().Select(ti => ti.Header).Aggregate(0, (acc, header) => acc ^ header.GetHashCode());
      var content = SpecialEditorMngr.ConstructEditorViewForObject(navh.Targets);
      if (!Equals(_specialEditorsHost.Content, content))
         _specialEditorsHost.Content = content;

      var newHash = SpecialEditorMngr.EditorsTabControl.Items.Cast<TabItem>().Select(ti => ti.Header).Aggregate(0, (acc, header) => acc ^ header.GetHashCode());

      if (content is TabControl tc)
         if (tabHash == newHash)
            SpecialEditorMngr.EditorsTabControl.SelectedIndex = selectedIndex;
         else
            tc.SelectedIndex = 0;
   }

   public static void UpdateSpecializedEditors()
   {
      var currNavh = NUINavigation.Instance.Current;
      if (currNavh != null)
         SetSpecializedEditors(currNavh);
   }
}