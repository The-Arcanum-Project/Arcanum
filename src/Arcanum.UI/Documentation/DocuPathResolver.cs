using System.IO;
using System.Windows;
using System.Windows.Threading;
using Arcanum.Core.GlobalStates;
using Arcanum.UI.AppFeatures;
using Common.Logger;

namespace Arcanum.UI.Documentation;

// if the app is build in debug mode we want to be able to load the documentation from a custom folder defined in the debug settings.
// otherwise if none is set we use the embedded ressources in the .exe file.
public static class DocuPathResolver
{
   private static Dictionary<FeatureId, DocuPage> _cache = new();
   private static FileSystemWatcher? _watcher;
   private static DispatcherTimer? _debounceTimer;

   private static bool IsUsingExternalSource { get; set; }
   private static string? ExternalPath { get; set; }

   // event to notify of reloaded files
   public static event Action? OnDocumentationReloaded;

   public static void LoadDocumentation(bool useExternal, bool forceReload = false, string? externalPath = null)
   {
      if (!forceReload && _cache.Count > 0)
         return; // Already loaded, skip unless forced

      _cache.Clear();

#if DEBUG
      var faildExternal = string.IsNullOrEmpty(externalPath) && !Directory.Exists(externalPath);
      if (faildExternal)
         ArcLog.Write("DPR", LogLevel.DBG, $"Could not find external path. Does the folder exist: {externalPath}");
      IsUsingExternalSource = useExternal && !faildExternal;
      ExternalPath = IsUsingExternalSource ? externalPath : null;
#else
        IsUsingExternalSource = false;
        ExternalPath = null;
#endif

      if (IsUsingExternalSource)
      {
         ReloadAll(); // Initial load from disk
         SetupWatcher(ExternalPath!);
      }
      else
      {
         _watcher?.Dispose();
         _watcher = null;
         LoadFromAssembly();
      }
   }

   private static void LoadFromAssembly()
   {
      // Find all resource names ending in .md
      var resourceNames = AppData.Assembly.GetManifestResourceNames()
                                 .Where(r => r.EndsWith(".md"));

      foreach (var name in resourceNames)
         using (var stream = AppData.Assembly.GetManifestResourceStream(name))
            if (stream != null)
            {
               using var reader = new StreamReader(stream);
               var page = DocuPageParser.Parse(reader); // YAML + MD parser
               if (page != null && !string.IsNullOrEmpty(page.Id.Value))
                  _cache[page.Id] = page;
            }
            else
               ArcLog.Write("DPR", LogLevel.ERR, $"Failed to load embedded documentation resource: {name}");
   }

   private static void ReloadAll()
   {
      try
      {
         var newCache = new Dictionary<FeatureId, DocuPage>();

         if (!IsUsingExternalSource)
         {
            LoadFromAssembly();
            return;
         }

         if (!Directory.Exists(ExternalPath))
            return;

         var files = Directory.GetFiles(ExternalPath, "*.md", SearchOption.AllDirectories);
         foreach (var file in files)
         {
            var page = DocuPageParser.Parse(file);
            if (page != null && !string.IsNullOrEmpty(page.Id.Value))
               newCache[page.Id] = page;
         }

         // Atomic swap to prevent UI thread from reading an empty dict during reload
         _cache = newCache;
         OnDocumentationReloaded?.Invoke();
      }
      catch (Exception ex)
      {
         ArcLog.Write("DPR", LogLevel.ERR, $"Error reloading documentation: {ex.Message}", ex);
      }
   }

   private static void SetupWatcher(string path)
   {
      _watcher?.Dispose();
#if DEBUG
      _watcher = new(path, "*.md")
      {
         IncludeSubdirectories = true, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
      };

      // wait 200ms after the last change event before reloading
      _debounceTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
      _debounceTimer.Tick += (_, _) =>
      {
         _debounceTimer.Stop();
         ReloadAll();
      };

      _watcher.Changed += (_, _) => Application.Current.Dispatcher.Invoke((Action)(() => _debounceTimer.Start()));
      _watcher.Created += (_, _) => Application.Current.Dispatcher.Invoke((Action)(() => _debounceTimer.Start()));
      _watcher.Deleted += (_, _) => Application.Current.Dispatcher.Invoke((Action)(() => _debounceTimer.Start()));

      _watcher.EnableRaisingEvents = true;
#endif
   }

   public static DocuPage[] GetAllDocuPages => _cache.Values.ToArray();
   public static DocuPage? GetPage(FeatureId id) => _cache.GetValueOrDefault(id);
}