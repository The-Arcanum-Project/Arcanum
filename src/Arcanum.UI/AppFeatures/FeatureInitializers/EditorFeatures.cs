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
                                         FeatureLocation.TopRight,
                                         FeatureScale.Major,
                                         [CommandScopes.GLOBAL, CommandScopes.EDITOR],
                                         ["Main UI", "Primary Window", "Workspace"],
                                         [
                                            new("Map", "Holds the center map of Arcanum which is used in basically all other features"),
                                            new("Status Bar",
                                                "Displays important information such as: hovered Location, Resource Usage, history status, and more."),
                                            new("Toolbar", "Contains buttons for common actions and tools, such as: saving, undo/redo, selection tools, etc."),
                                            new("NUI", "The interface where you can browse all properties of an object."), new("Specialized Editors",
                                               "Hosts specialized editors for specific types of content, such as PopEditor, PoliticalEditor, etc."),
                                         ],
                                         [new("Eu5 Modding Wiki Entry", "https://eu5.paradoxwikis.com/Arcanum", ReferenceType.Wiki)],
                                         VersionNumbers.V107,
                                         FeatureStatus.Stable,
                                         null).AddToRegistry();
   }

   public static AppFeature MainWindowFeature { get; private set; } = null!;
}