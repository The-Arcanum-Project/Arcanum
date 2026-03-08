using Arcanum.Core.Utils;
using Arcanum.UI.Commands;

namespace Arcanum.UI.AppFeatures.FeatureInitializers;

public static class SpecializedEditorFeatures
{
   public static AppFeature InstitutionEditor { get; private set; } = null!;

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
   }
}