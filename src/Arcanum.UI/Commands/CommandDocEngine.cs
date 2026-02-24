using System.Text;
using Arcanum.Core.CoreSystems.IO;
using Arcanum.UI.Components.Converters;

namespace Arcanum.UI.Commands;

public static class CommandDocEngine
{
   public static string ExportJson()
   {
      var data = CommandRegistry.AllCommands.Select(c => new
      {
         c.Id.Value,
         c.DisplayName,
         c.Description,
         c.Scope,
         DefaultShortcuts = c.DefaultGestures.Select(GestureToTextConverter.DefaultConvert),
      });
      return JsonProcessor.Serialize(data);
   }

   public static string ExportHtml()
   {
      var sb = new StringBuilder();
      sb.AppendLine("<html><head><style>body { background: #2B2B2B; color: #BBBBBB; font-family: sans-serif; } table { width: 100%; border-collapse: collapse; } th, td { border: 1px solid #393939; padding: 8px; text-align: left; } th { background: #3C3F41; }</style></head><body>");

      sb.AppendLine("<h1>Arcanum Command Reference</h1>");
      sb.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd}</p>");
      var categories = CommandRegistry.AllCommands
                                      .GroupBy(c => c.Scope)
                                      .OrderBy(g => g.Key);

      foreach (var group in categories)
      {
         sb.AppendLine($"<h2>{group.Key}</h2>");
         sb.AppendLine("<table><tr><th>Name</th><th>Shortcut</th><th>Scope</th><th>ID</th><th>Description</th></tr>");

         foreach (var cmd in group.OrderBy(c => c.DisplayName))
         {
            var shortcuts = cmd.Gestures.Count > 0 ? string.Join(", ", cmd.Gestures.Select(GestureToTextConverter.DefaultConvert)) : "None";
            sb.AppendLine($"<tr><td><strong>{cmd.DisplayName}</strong></td><td><code>{shortcuts}</code></td><td>{cmd.Scope}</td><td><code>{cmd.Id.Value}</code></td><td>{cmd.Description}</td></tr>");
         }

         sb.AppendLine("</table>");
      }

      sb.AppendLine("</body></html>");
      return sb.ToString();
   }

   public static string ExportMarkdown()
   {
      var sb = new StringBuilder();
      sb.AppendLine("# Arcanum Command Reference");
      sb.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd}");
      sb.AppendLine();

      var categories = CommandRegistry.AllCommands
                                      .GroupBy(c => c.Scope)
                                      .OrderBy(g => g.Key);

      foreach (var group in categories)
      {
         sb.AppendLine($"## {group.Key}");
         sb.AppendLine("| Name | Shortcut | Scope | ID | Description |");
         sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

         foreach (var cmd in group.OrderBy(c => c.DisplayName))
         {
            var shortcuts = cmd.Gestures.Count > 0 ? string.Join(", ", cmd.Gestures.Select(GestureToTextConverter.DefaultConvert)) : "None";
            sb.AppendLine($"| **{cmd.DisplayName}** | `{shortcuts}` | {cmd.Scope} | `{cmd.Id.Value}` | {cmd.Description} |");
         }

         sb.AppendLine();
      }

      return sb.ToString();
   }
}