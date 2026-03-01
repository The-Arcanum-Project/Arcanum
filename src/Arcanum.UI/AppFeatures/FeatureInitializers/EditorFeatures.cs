using Arcanum.Core.Utils;
using Arcanum.UI.Commands;

namespace Arcanum.UI.AppFeatures.FeatureInitializers;

public static class EditorFeatures
{
   public static void Initialize()
   {
      MainWindowFeature = new AppFeature(FeatureIds.Editor.MainWindow,
                                         "Main Window",
                                         "The main window of the application, containing the primary UI elements and workspace for users to interact with.",
                                         FeatureCategory.Editor,
                                         FeatureLevel.System,
                                         null,
                                         FeatureLocation.Center,
                                         FeatureScale.Full,
                                         [CommandScopes.GLOBAL, CommandScopes.EDITOR],
                                         ["Main UI", "Primary Window", "Workspace"],
                                         [
                                            new("Map", "Holds the center map of Arcanum which is used in basically all other features"), new("Status Bar",
                                               "Displays important information such as: hovered Location, Resource Usage, history status, and more."),
                                            new("Toolbar", "Contains buttons for common actions and tools, such as: saving, undo/redo, selection tools, etc."),
                                            new("NUI", "The interface where you can browse all properties of an object."), new("Specialized Editors",
                                               "Hosts specialized editors for specific types of content, such as PopEditor, PoliticalEditor, etc."),
                                         ],
                                         [new("Eu5 Modding Wiki Entry", "https://eu5.paradoxwikis.com/Arcanum", ReferenceType.Wiki)],
                                         VersionNumbers.V107,
                                         FeatureStatus.Stable,
                                         null).AddToRegistry();

      MapFeature = new AppFeature(FeatureIds.Editor.Map,
                                  "Map",
                                  "The central map feature of the editor, allowing users to view and interact with the mods map. Depending on the set map mode it can be used for a variety of tasks such.",
                                  FeatureCategory.Editor,
                                  FeatureLevel.Module,
                                  FeatureIds.Editor.MainWindow,
                                  FeatureLocation.Center,
                                  FeatureScale.Major,
                                  [CommandScopes.GLOBAL, CommandScopes.EDITOR],
                                  ["Map View", "Main Map", "World Map"],
                                  [
                                     new("Interactivity",
                                         "The map behaves just like the ingame map with the addition of selection capabilities. " +
                                         "These can be activated using the different modifier keys (Ctrl, Shift, Alt)."),
                                     new("Location Interaction",
                                         "Clicking on a location opens the NUI with that location selected, allowing you to view and edit its properties. " +
                                         "Any amount of locations can be selected at once, and the NUI will show a combined view of all selected locations where only shared values are visible."),
                                     new("Hover Information",
                                         "Hovering over a location shows a tooltip with key information about that location, depending on the current Map Mode."),
                                     new("Map Modes",
                                         "The map supports different modes (e.g., Political, Location, Topography, etc.) that change the visual representation " +
                                         "and available interactions based on the selected mode."),
                                     new("Quick Actions",
                                         "Right-clicking on a location opens a context menu with quick actions relevant to that location, such as convenient selection shortcuts, " +
                                         "map exporting options, and more."),
                                  ],
                                  [],
                                  VersionNumbers.V107,
                                  FeatureStatus.Stable,
                                  null).AddToRegistry();
   }

   public static AppFeature MainWindowFeature { get; private set; } = null!;
   public static AppFeature MapFeature { get; private set; } = null!;
}