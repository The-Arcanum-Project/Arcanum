namespace Arcanum.UI.Commands;

public static class CommandIds
{
   // "ui"
   public static class UI
   {
      private const string PATH = nameof(UI);

      // "ui.window"
      public static class Window
      {
         private const string WINDOW_PATH = $"{PATH}.{nameof(Window)}";

         // "ui.window.close"
         public static readonly CommandId Close = CommandId.Create(WINDOW_PATH);

         // "ui.window.minimize"
         public static readonly CommandId Minimize = CommandId.Create(WINDOW_PATH);
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