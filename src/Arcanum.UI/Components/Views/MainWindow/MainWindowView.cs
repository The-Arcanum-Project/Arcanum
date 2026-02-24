using Arcanum.UI.AppFeatures;
using Arcanum.UI.Commands;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arcanum.UI.Components.Views.MainWindow;

public class MainWindowView : ObservableObject, IAppFeature
{
   public FeatureId Id => FeatureIds.Editor.MainWindow;
   public string DisplayName => "Main Window";
   public string Description => "The main window of the application, containing the primary UI elements and workspace for users to interact with.";
   public FeatureCategory Category => FeatureCategory.Editor;
   public FeatureLevel Level => FeatureLevel.System;
   public FeatureId? ParentFeatureId => null;
   public FeatureLocation Location => FeatureLocation.Center;
   public FeatureScale Scale => FeatureScale.Full;
   public IEnumerable<string> AssociatedScopes { get; } = [CommandScopes.GLOBAL, CommandScopes.EDITOR];
   public IEnumerable<string> SearchSynonyms { get; } = ["Main UI", "Primary Window", "Workspace"];
   public IEnumerable<FeatureNote> QuickPoints { get; } =
   [
      new("Map", "Holds the center map of Arcanum which is used in basically all other features"),
      new("Status Bar", "Displays important information such as: hovered Location, Resource Usage, history status, and more."),
      new("Toolbar", "Contains buttons for common actions and tools, such as: saving, undo/redo, selection tools, etc."),
      new("NUI", "The interface where you can browse all properties of an object."),
      new("Specialized Editors", "Hosts specialized editors for specific types of content, such as PopEditor, PoliticalEditor, etc."),
   ];
   public IEnumerable<ExternalReference>? Links => [new("Eu5 Modding Wiki Entry", "https://eu5.paradoxwikis.com/Arcanum", ReferenceType.Wiki)];
   public string IntroducedIn => "1.0.7";
   public FeatureStatus Status => FeatureStatus.Stable;
}