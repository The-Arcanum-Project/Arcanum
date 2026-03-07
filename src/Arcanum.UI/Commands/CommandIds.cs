namespace Arcanum.UI.Commands;

public static class CommandIds
{
   public static class Global
   {
      private const string PATH = nameof(Global);

      // "global.open_context_explorer_window"
      public static readonly CommandId OpenContextExplorerWindow = CommandId.Create(PATH);

      // "global.open_context_explorer_control"
      public static readonly CommandId OpenControlExplorer = CommandId.Create(PATH);

      // "global.open_help"
      public static readonly CommandId OpenHelp = CommandId.Create(PATH);
   }

   // "ui"
   public static class UI
   {
      private const string PATH = nameof(UI);

      // "ui.test_command"
      public static readonly CommandId TestCommand = CommandId.Create(PATH);

      // "ui.window"
      public static class Window
      {
         private const string WINDOW_PATH = $"{PATH}.{nameof(Window)}";

         // "ui.window.close"
         public static readonly CommandId Close = CommandId.Create(WINDOW_PATH);

         // "ui.window.minimize"
         public static readonly CommandId Minimize = CommandId.Create(WINDOW_PATH);

         // "ui.window.maximize"
         public static readonly CommandId Maximize = CommandId.Create(WINDOW_PATH);

         public static class Layout
         {
            private const string LAYOUT_PATH = $"{WINDOW_PATH}.{nameof(Layout)}";

            // "ui.window.layout.save"
            public static readonly CommandId Save = CommandId.Create(LAYOUT_PATH);

            // "ui.window.layout.load"
            public static readonly CommandId Load = CommandId.Create(LAYOUT_PATH);
         }
      }
   }

   // "editor"
   public static class Editor
   {
      private const string PATH = nameof(Editor);

      // "editor.open_queastor"
      public static readonly CommandId OpenQueastor = CommandId.Create(PATH);

      // "editor.comment"
      public static readonly CommandId Comment = CommandId.Create(PATH);

      public static class Map
      {
         private const string MAP_PATH = $"{PATH}.{nameof(Map)}";

         // "editor.map.rectangle_select_modifier"
         public static readonly CommandId RectangleSelectModifier = CommandId.Create(MAP_PATH);
      }

      public static class SpecializedEditors
      {
         private const string SPE_PATH = $"{PATH}.{nameof(SpecializedEditors)}";

         public static class PoliticalEditor
         {
            private const string PE_PATH = $"{SPE_PATH}.{nameof(PoliticalEditor)}";

            // "editor.specialized_editors.political_editor.sync_with_search"
            public static readonly CommandId SyncWithSelection = CommandId.Create(PE_PATH);
         }
      }
   }

   // "help_window"
   public static class HelpWindow
   {
      private const string PATH = nameof(HelpWindow);

      // "help_window.open_feature_explorer_for_feature"
      public static readonly CommandId OpenFeatureExplorerForFeature = CommandId.Create(PATH);

      public static class DashBoardView
      {
         private const string DASHBOARD_PATH = $"{PATH}.{nameof(DashBoardView)}";

         // "help_window.dashboard_view.next_tip"
         public static readonly CommandId NextRandomTip = CommandId.Create(DASHBOARD_PATH);

         // "help_window.dashboard_view.previous_tip"
         public static readonly CommandId PreviousRandomTip = CommandId.Create(DASHBOARD_PATH);
      }
   }

   // "file"
   public static class File
   {
      private const string PATH = nameof(File);

      // "file.save"
      public static readonly CommandId Save = CommandId.Create(PATH);
   }
}