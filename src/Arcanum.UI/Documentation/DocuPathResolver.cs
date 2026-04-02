#region

using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.AppFeatures;
using Common.Logger;

#endregion

namespace Arcanum.UI.Documentation;

public static class DocuPathResolver
{
   private const string SNIPPET_FOLDER_NAME = "Snippets";
   private static Dictionary<FeatureId, DocuPage> _cache = new();
   private static Dictionary<string, string> _snippets = new(StringComparer.OrdinalIgnoreCase);

   private static FileSystemWatcher? _watcher;
   private static FileSystemWatcher? _snippetsWatcher;
   private static DispatcherTimer? _debounceTimer;

   private static bool IsUsingExternalSource { get; set; }
   private static string? ExternalPath { get; set; }
   private static string? ExternalSnippetsPath { get; set; }

   public static DocuPage[] GetAllDocuPages => _cache.Values.ToArray();
   public static string[] GetAllSnippetIds => _snippets.Keys.ToArray();

   public static event Action? OnDocumentationReloaded;

   public static void LoadDocumentation(bool useExternal, bool forceReload = false, string? externalPath = null)
   {
      if (!forceReload && _cache.Count > 0)
         return;

      _cache.Clear();
      _snippets.Clear();

#if DEBUG
      var failedExternal = string.IsNullOrEmpty(externalPath) && !Directory.Exists(externalPath);
      if (failedExternal)
         ArcLog.Write("DPR", LogLevel.DBG, $"Could not find external docs path: {externalPath}");

      IsUsingExternalSource = useExternal && !failedExternal;
      ExternalPath = IsUsingExternalSource ? externalPath : null;
      ExternalSnippetsPath = IsUsingExternalSource ? Path.Combine(ExternalPath!, SNIPPET_FOLDER_NAME) : null;
#else
      IsUsingExternalSource = false;
      ExternalPath = null;
      ExternalSnippetsPath = null;
#endif

      if (IsUsingExternalSource)
      {
         ReloadAll();
         SetupWatchers(ExternalPath!, ExternalSnippetsPath);
      }
      else
      {
         _watcher?.Dispose();
         _watcher = null;
         _snippetsWatcher?.Dispose();
         _snippetsWatcher = null;

         LoadFromAssembly();
      }
   }

   private static void LoadFromAssembly(Dictionary<FeatureId, DocuPage>? targetCache = null, Dictionary<string, string>? targetSnippets = null)
   {
      var activeCache = targetCache ?? _cache;
      var activeSnippets = targetSnippets ?? _snippets;

      var resourceNames = AppData.Assembly.GetManifestResourceNames().Where(r => r.EndsWith(".md"));

      foreach (var name in resourceNames)
      {
         using var stream = AppData.Assembly.GetManifestResourceStream(name);
         if (stream == null)
            continue;

         using var reader = new StreamReader(stream);

         // Determine if it is a snippet or a document. 
         // Embedded snippets are kept in a folder/namespace containing ".SNIPPET_FOLDER_NAME."
         if (name.Contains(".SNIPPET_FOLDER_NAME.", StringComparison.OrdinalIgnoreCase))
         {
            // e.g., Arcanum.UI.Assets.SNIPPET_FOLDER_NAME.my_warning.md -> "my_warning"
            var parts = name.Split('.');
            var snippetId = parts[^2];
            activeSnippets[snippetId] = reader.ReadToEnd();
         }
         else
         {
            var page = DocuPageParser.Parse(reader);
            if (page != null && !string.IsNullOrEmpty(page.Id.Value))
               activeCache[page.Id] = page;
         }
      }
   }

   public static void ReloadAll()
   {
      try
      {
         var newCache = new Dictionary<FeatureId, DocuPage>();
         var newSnippets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

         if (!IsUsingExternalSource)
            LoadFromAssembly(newCache, newSnippets);
         else
         {
            // Load Documentation Pages
            if (Directory.Exists(ExternalPath))
            {
               var files = Directory.GetFiles(ExternalPath, "*.md", SearchOption.AllDirectories);
               foreach (var file in files)
               {
                  var page = DocuPageParser.Parse(file);
                  if (page != null && !string.IsNullOrEmpty(page.Id.Value))
                  {
                     page.SourcePath = file;
                     newCache[page.Id] = page;
                  }
               }
            }

            // Load Snippets
            if (!string.IsNullOrEmpty(ExternalSnippetsPath) && Directory.Exists(ExternalSnippetsPath))
            {
               var files = Directory.GetFiles(ExternalSnippetsPath, "*.md", SearchOption.AllDirectories);
               foreach (var file in files)
               {
                  var snippetId = Path.GetFileNameWithoutExtension(file);
                  newSnippets[snippetId] = File.ReadAllText(file);
               }
            }
         }

         // Atomic swap
         _cache = newCache;
         _snippets = newSnippets;

         OnDocumentationReloaded?.Invoke();
      }
      catch (Exception ex)
      {
         ArcLog.Write("DPR", LogLevel.ERR, $"Error reloading documentation: {ex.Message}", ex);
      }
   }

   private static void SetupWatchers(string docsPath, string? snippetsPath)
   {
      _watcher?.Dispose();
      _snippetsWatcher?.Dispose();
#if DEBUG
      // Single debouncer for both folders
      _debounceTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
      _debounceTimer.Tick += (_, _) =>
      {
         _debounceTimer.Stop();
         ReloadAll();
      };

      void TriggerDebounce() => Application.Current.Dispatcher.Invoke(() =>
      {
         _debounceTimer?.Stop();
         _debounceTimer?.Start();
      });

      // Watch Docs Folder
      _watcher = new(docsPath, "*.md")
      {
         IncludeSubdirectories = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
      };
      _watcher.Changed += (_, _) => TriggerDebounce();
      _watcher.Created += (_, _) => TriggerDebounce();
      _watcher.Deleted += (_, _) => TriggerDebounce();
      _watcher.EnableRaisingEvents = true;

      // Watch Snippets Folder (if exists)
      if (!string.IsNullOrEmpty(snippetsPath) && Directory.Exists(snippetsPath))
      {
         _snippetsWatcher = new(snippetsPath, "*.md")
         {
            IncludeSubdirectories = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
         };
         _snippetsWatcher.Changed += (_, _) => TriggerDebounce();
         _snippetsWatcher.Created += (_, _) => TriggerDebounce();
         _snippetsWatcher.Deleted += (_, _) => TriggerDebounce();
         _snippetsWatcher.EnableRaisingEvents = true;
      }
#endif
   }

   public static DocuPage? GetPage(FeatureId id) => _cache.GetValueOrDefault(id);

   public static string ProcessSnippets(string rawMarkdown)
   {
      if (string.IsNullOrEmpty(rawMarkdown))
         return rawMarkdown;

      const string pattern = @"\{\{snippet:(.*?)\}\}";
      const int maxDepth = 20;
      var currentDepth = 0;

      while (Regex.IsMatch(rawMarkdown, pattern, RegexOptions.IgnoreCase))
      {
         if (currentDepth >= maxDepth)
         {
            ArcLog.Write("DPR", LogLevel.WRN, "Snippet expansion aborted: Infinite recursion detected.");

            rawMarkdown = Regex.Replace(rawMarkdown,
                                        pattern,
                                        match => $"*`Error: Infinite recursion detected in snippet '{match.Groups[1].Value.Trim()}'`*",
                                        RegexOptions.IgnoreCase);
            break;
         }

         rawMarkdown = Regex.Replace(rawMarkdown,
                                     pattern,
                                     match =>
                                     {
                                        var snippetId = match.Groups[1].Value.Trim();

                                        if (_snippets.TryGetValue(snippetId, out var snippetContent))
                                           return snippetContent;

                                        return $"*`Error: Snippet '{snippetId}' not found`*";
                                     },
                                     RegexOptions.IgnoreCase);

         currentDepth++;
      }

      return rawMarkdown;
   }
}