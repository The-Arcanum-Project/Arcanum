using Arcanum.Core.Utils;
using Arcanum.UI.Commands;

namespace Arcanum.UI.AppFeatures.FeatureInitializers;

public static class SpecializedEditorFeatures
{
   public static AppFeature InstitutionEditor { get; private set; } = null!;
   public static AppFeature PoliticalEditor { get; private set; } = null!;

   public static void Initialize()
   {
      InstitutionEditor = new AppFeature(FeatureIds.Editor.SpecializedEditors.InstitutionEditor,
                                         "Institution Editor",
                                         "Allows setting the institutions which are present in a location. Supports any amount of locations at once.",
                                         FeatureCategory.SpecializedEditor,
                                         FeatureLevel.Panel,
                                         FeatureIds.Editor.MainWindow,
                                         FeatureLocation.Right,
                                         FeatureScale.Standard,
                                         [CommandScopes.GLOBAL, CommandScopes.INSTITUTION_EDITOR],
                                         ["Institution"],
                                         [
                                            new("Handling",
                                                "Each institution is represented in a row. There are `3` statuses for each institution: Present in all, Present in some, Not present in any. " +
                                                "The status can be always advanced by clicking on the icon. It can change from Not present in any to present in all aswell as " +
                                                "present in some to not present in any and from prsent in some to present in all."),
                                            new("Reversability",
                                                $"Any changes taken can be undone using the undo system, which can be accessed by pressing " +
                                                $"{CommandRegistry.GetDisplayStringForCommand(CommandIds.Global.Undo)} or {CommandRegistry.GetDisplayStringForCommand(CommandIds.Global.Redo)}."),
                                         ],
                                         [],
                                         VersionNumbers.V1072,
                                         FeatureStatus.Beta,
                                         null).AddToRegistry();

      PoliticalEditor = new AppFeature(FeatureIds.Editor.SpecializedEditors.PoliticalEditor,
                                       "Political Editor",
                                       "Allows editing the ownership of locations. Any number of locations can be added / removed from a single country at once.",
                                       FeatureCategory.SpecializedEditor,
                                       FeatureLevel.Panel,
                                       FeatureIds.Editor.MainWindow,
                                       FeatureLocation.Right,
                                       FeatureScale.Standard,
                                       [CommandScopes.GLOBAL, CommandScopes.POLITICAL_EDITOR],
                                       ["Political", "Owner", "Controller", "Control", "Integrated"],
                                       [
                                          new("Handling",
                                              "To use the editor, a country has to be selected first. This can either be done via the provided dropdown, the Queastor or by using the context menu" +
                                              "on the map while the political map mode is active. After a country is selected all its owned / controlled / integrated locations will be shown in the editor." +
                                              "To add or remove locations from the country simply select the desired locations on the map and press the corresponding button in the editor."),
                                          new("Reversability",
                                              $"Any changes taken can be undone using the undo system, which can be accessed by pressing " +
                                              $"{CommandRegistry.GetDisplayStringForCommand(CommandIds.Global.Undo)} or {CommandRegistry.GetDisplayStringForCommand(CommandIds.Global.Redo)}."),
                                          new("Filters",
                                              "There are several filter options to modify the behavior of adding ownership. By default the selected locations will all simply be added to the " +
                                              "target. \nBy enabling the 'Star' filter all locations will be removed from everywhere else they are present in and then added to the country.\n" +
                                              "By enableing the the 'Stars' option all locations will only be removed from the same target collection in all other countries."),
                                          new("Filter Bar",
                                              "Using the filter bar on each collection you can quickly check if a location is already present in one or not. This does not effect any behavior."),
                                       ],
                                       [],
                                       VersionNumbers.V1072,
                                       FeatureStatus.Stable,
                                       null).AddToRegistry();
   }
}