using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;

namespace Arcanum.Core.CoreSystems.IO;

public static class IO
{
   private static readonly Encoding Windows1250Encoding;
   private static readonly UTF8Encoding BomUtf8Encoding;
   private static readonly UTF8Encoding NoBomUtf8Encoding; // Standard UTF-8 (no BOM)

   public static string GetUserDocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

   // TODO fix this to EU5 once we know the correct path
   public static string GetUserModFolderPath => Path.Combine(GetUserDocumentsPath, "Paradox Interactive", "Europa Universalis V", "mod");

   static IO()
   {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      Windows1250Encoding = Encoding.GetEncoding("windows-1250");
      BomUtf8Encoding = new(true); // UTF-8 with BOM
      NoBomUtf8Encoding = new(false); // UTF-8 without BOM (same as Encoding.UTF8 default)
   }

   public static string GetArcanumDataPath => Path.Combine(GetUserDocumentsPath, "Arcanum");
   public static string GetConfigPath => Path.Combine(GetArcanumDataPath, "Config");
   public static string GetLogsPath => Path.Combine(GetArcanumDataPath, "Logs");
   public static string GetCrashLogsPath => Path.Combine(GetLogsPath, "CrashLogs");
   public static string GetErrorLogsFilePath => Path.Combine(GetLogsPath, Config.Settings.ErrorLogOptions.ErrorLogFileName);
   public static string GetMapExportPath => Path.Combine(GetArcanumDataPath, "MapExports");

   // Directory Utils
   public static List<string> GetDirectories(string path,
                                             string searchString = "*",
                                             SearchOption searchOption = SearchOption.TopDirectoryOnly)
   {
      if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
         return [];

      try
      {
         return Directory.GetDirectories(path, searchString, searchOption).ToList();
      }
      catch (IOException)
      {
         throw new IOException($"Failed to get directories from path: {path}");
      }
      catch (UnauthorizedAccessException)
      {
         throw new UnauthorizedAccessException($"Access denied to path: {path}");
      }
   }

   // --- Dialogs ---
   public static string? SelectFolder(string startPath, string defaultFileName = "Select Folder")
   {
      EnsureDirectoryExists(startPath);

      using var dialog = new OpenFileDialog();
      dialog.InitialDirectory = startPath;
      dialog.CheckFileExists = false; // 
      dialog.CheckPathExists = true;
      dialog.FileName = defaultFileName;
      dialog.Title = "Select Folder";

      if (dialog.ShowDialog() == DialogResult.OK)
         return Path.GetDirectoryName(dialog.FileName);

      return null;
   }

   public static string? SelectFile(string startFolder, string filterText)
   {
      EnsureDirectoryExists(startFolder);

      using var dialog = new OpenFileDialog();
      dialog.InitialDirectory = startFolder;
      dialog.CheckFileExists = true;
      dialog.CheckPathExists = true;
      dialog.Filter = filterText;
      dialog.Title = "Select File";

      if (dialog.ShowDialog() == DialogResult.OK)
         return dialog.FileName;

      return null;
   }

   // --- Generic File Read Operations ---
   public static string? ReadAllText(string path, Encoding encoding)
   {
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return null;

      try
      {
         return File.ReadAllText(path, encoding);
      }
      catch (IOException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (UnauthorizedAccessException)
      {
         /* TODO: Log error */
         return null;
      }
   }

   public static ReadOnlySpan<char> ReadAllTextAsSpan(string path, Encoding encoding)
   {
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return default;

      try
      {
         return File.ReadAllText(path, encoding).AsSpan();
      }
      catch (IOException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (UnauthorizedAccessException)
      {
         /* TODO: Log error */
         return null;
      }
   }

   public static string[]? ReadAllLines(string path, Encoding encoding)
   {
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return null;

      try
      {
         return File.ReadAllLines(path, encoding);
      }
      catch (IOException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (UnauthorizedAccessException)
      {
         /* TODO: Log error */
         return null;
      }
   }

   // --- Specific Encoding Readers ---
   public static string? ReadAllTextAnsi(string path) => ReadAllText(path, Windows1250Encoding);
   public static string[]? ReadAllLinesAnsi(string path) => ReadAllLines(path, Windows1250Encoding);
   public static string? ReadAllTextUtf8(string path) => ReadAllText(path, NoBomUtf8Encoding);
   public static string[]? ReadAllLinesUtf8(string path) => ReadAllLines(path, NoBomUtf8Encoding);
   public static string? ReadAllTextUtf8WithBom(string path) => ReadAllText(path, BomUtf8Encoding);
   public static string[]? ReadAllLinesUtf8WithBom(string path) => ReadAllLines(path, BomUtf8Encoding);

   // --- Generic File Write Operations ---
   public static bool WriteAllText(string path, string data, Encoding encoding, bool append = false)
   {
      if (string.IsNullOrEmpty(path))
         return false;

      try
      {
         FileStateManager.IgnoreNextChange(path);
         if (!EnsureFileDirectoryExists(path))
            return false;

         if (append)
            File.AppendAllText(path, data, encoding);
         else
            File.WriteAllText(path, data, encoding);

#if DEBUG
         ArcLog.WriteLine("SAV", LogLevel.INF, $"IO.WriteAllText succeeded for path: {path}");
#endif
         return true;
      }
      catch (IOException e)
      {
         ArcLog.WriteLine("SAV", LogLevel.CRT, $"IO.WriteAllText failed for path: {path}\n{e}");
         return false;
      }
      catch (UnauthorizedAccessException)
      {
         ArcLog.WriteLine("SAV", LogLevel.CRT, $"IO.WriteAllText unauthorized access for path: {path}");
         return false;
      }
   }

   // --- Specific Encoding Writers ---
   public static bool WriteAllTextAnsi(string path, string data, bool append = false) => WriteAllText(path, data, Windows1250Encoding, append);

   public static bool WriteAllTextUtf8(string path, string data, bool append = false) => WriteAllText(path, data, NoBomUtf8Encoding, append);

   public static bool WriteAllTextUtf8WithBom(string path, string data, bool append = false) => WriteAllText(path, data, BomUtf8Encoding, append);

   // --- Directory Operations ---
   public static bool EnsureDirectoryExists(string directoryPath)
   {
      if (string.IsNullOrEmpty(directoryPath))
         return false;

      try
      {
         if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

         return true;
      }
      catch (Exception)
      {
         /* TODO: Log error (e.g., UnauthorizedAccessException) */
         return false;
      }
   }

   private const bool ALLOW_DEFAULT_TO_APP_DIRECTORY = true;

   public static bool EnsureFileDirectoryExists(string filePath)
   {
      if (string.IsNullOrEmpty(filePath))
         return false;

      var directoryPath = Path.GetDirectoryName(filePath);
      if (string.IsNullOrEmpty(directoryPath))
         // This case means filePath is just a filename like "file.txt" without a path.
         // Assuming it implies the current working directory, which should already exist.
         // Or, it could be an invalid path. For EnsureFileDirectoryExists, if there's no
         // directory part, there's nothing to ensure/create.
         return ALLOW_DEFAULT_TO_APP_DIRECTORY; // Or false, depending on desired strictness. 

      return EnsureDirectoryExists(directoryPath);
   }

   // --- File/Directory Checks ---
   public static bool FileExists(string path) => !string.IsNullOrEmpty(path) && File.Exists(path);
   public static bool DirectoryExists(string path) => !string.IsNullOrEmpty(path) && Directory.Exists(path);

   // --- Image Operations ---
   public static bool SaveBitmap(string path, Bitmap bmp, ImageFormat format)
   {
      if (string.IsNullOrEmpty(path) || bmp == null! || format == null!)
         return false;

      try
      {
         if (!EnsureFileDirectoryExists(path))
            return false;

         bmp.Save(path, format);
         return true;
      }
      catch (Exception)
      {
         /* TODO: Log error */
         return false;
      }
   }

   // --- Async Operations ---
   public static async Task<string?> ReadAllTextAsync(string path,
                                                      Encoding encoding,
                                                      CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return null;

      try
      {
         return await File.ReadAllTextAsync(path, encoding, cancellationToken);
      }
      catch (IOException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (UnauthorizedAccessException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (OperationCanceledException)
      {
         /* TODO: Log cancellation */
         return null;
      }
   }

   public static async Task<string[]?> ReadAllLinesAsync(string path,
                                                         Encoding encoding,
                                                         CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return null;

      try
      {
         return await File.ReadAllLinesAsync(path, encoding, cancellationToken);
      }
      catch (IOException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (UnauthorizedAccessException)
      {
         /* TODO: Log error */
         return null;
      }
      catch (OperationCanceledException)
      {
         /* TODO: Log cancellation */
         return null;
      }
   }

   public static Task<string?> ReadAllTextAnsiAsync(string path, CancellationToken cancellationToken = default)
      => ReadAllTextAsync(path, Windows1250Encoding, cancellationToken);

   public static Task<string[]?> ReadAllLinesAnsiAsync(string path, CancellationToken cancellationToken = default)
      => ReadAllLinesAsync(path, Windows1250Encoding, cancellationToken);

   public static Task<string?> ReadAllTextUtf8Async(string path, CancellationToken cancellationToken = default)
      => ReadAllTextAsync(path, NoBomUtf8Encoding, cancellationToken);

   public static Task<string[]?> ReadAllLinesUtf8Async(string path, CancellationToken cancellationToken = default)
      => ReadAllLinesAsync(path, NoBomUtf8Encoding, cancellationToken);

   public static Task<string?> ReadAllTextUtf8WithBomAsync(string path,
                                                           CancellationToken cancellationToken = default)
      => ReadAllTextAsync(path, BomUtf8Encoding, cancellationToken);

   public static Task<string[]?> ReadAllLinesUtf8WithBomAsync(string path,
                                                              CancellationToken cancellationToken = default)
      => ReadAllLinesAsync(path, BomUtf8Encoding, cancellationToken);

   public static async Task<bool> WriteAllTextAsync(string path,
                                                    string data,
                                                    Encoding encoding,
                                                    bool append = false,
                                                    CancellationToken cancellationToken = default)
   {
      if (string.IsNullOrEmpty(path))
         return false;

      try
      {
         if (!EnsureFileDirectoryExists(path))
            return false;

         if (append)
            await File.AppendAllTextAsync(path, data, encoding, cancellationToken);
         else
            await File.WriteAllTextAsync(path, data, encoding, cancellationToken);
         return true;
      }
      catch (IOException)
      {
         /* TODO: Log error */
         return false;
      }
      catch (UnauthorizedAccessException)
      {
         /* TODO: Log error */
         return false;
      }
      catch (OperationCanceledException)
      {
         /* TODO: Log cancellation */
         return false;
      }
   }

   public static Task<string[]> ReadLineRange(string path,
                                              int lineStart,
                                              int lineEnd,
                                              CancellationToken cancellationToken = default)
   {
      ArgumentNullException.ThrowIfNull(path);
      if (lineStart <= 0)
         throw new ArgumentOutOfRangeException(nameof(lineStart), "Starting line number must be positive.");
      if (lineEnd < lineStart)
         throw new ArgumentOutOfRangeException(nameof(lineEnd),
                                               "Ending line number must be greater than or equal to the starting line number.");
      if (!File.Exists(path))
         throw new FileNotFoundException("File not found.", path);

      return Task.Run(() =>
                      {
                         var result = new string[lineEnd - lineStart + 1];
                         var currentLineNumber = 1;
                         var numAdded = 0;

                         using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                         using var sr = new StreamReader(fs);
                         while (sr.ReadLine() is { } line)
                         {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (currentLineNumber >= lineStart)
                               result[numAdded++] = line;

                            if (currentLineNumber >= lineEnd)
                               break;

                            currentLineNumber++;
                         }

                         return result;
                      },
                      cancellationToken);
   }

   public static Task<bool> WriteAllTextAnsiAsync(string path,
                                                  string data,
                                                  bool append = false,
                                                  CancellationToken cancellationToken = default)
      => WriteAllTextAsync(path, data, Windows1250Encoding, append, cancellationToken);

   public static Task<bool> WriteAllTextUtf8Async(string path,
                                                  string data,
                                                  bool append = false,
                                                  CancellationToken cancellationToken = default)
      => WriteAllTextAsync(path, data, NoBomUtf8Encoding, append, cancellationToken);

   public static Task<bool> WriteAllTextUtf8WithBomAsync(string path,
                                                         string data,
                                                         bool append = false,
                                                         CancellationToken cancellationToken = default)
      => WriteAllTextAsync(path, data, BomUtf8Encoding, append, cancellationToken);

   public static Task<bool> SaveBitmapAsync(string path,
                                            Bitmap bmp,
                                            ImageFormat format,
                                            CancellationToken cancellationToken = default)
   {
      // Bitmap.Save is synchronous. Run it on a thread pool thread.
      return Task.Run(() => SaveBitmap(path, bmp, format), cancellationToken);
   }

   #region StreamReaders

   /// <summary>
   /// Creates a StreamReader for the specified path with the given encoding.
   /// Returns true if successful, false otherwise.
   /// The <see cref="StreamReader"/> will have to be disposed by the caller.
   /// </summary>
   /// <param name="path"></param>
   /// <param name="encoding"></param>
   /// <param name="reader"></param>
   /// <returns></returns>
   public static bool CreateStreamReader(string path, Encoding encoding, out StreamReader? reader)
   {
      reader = null;
      if (string.IsNullOrEmpty(path) || !File.Exists(path))
         return false;

      try
      {
         reader = new(path, encoding);
         return true;
      }
      catch (IOException)
      {
         return false;
      }
      catch (UnauthorizedAccessException)
      {
         return false;
      }
   }

   #endregion

   public static string GetNextAvailableFileName(string name, string folder)
   {
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(folder))
         return name;

      var filePath = Path.Combine(folder, name);
      if (!File.Exists(filePath))
         return name;

      var fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
      var extension = Path.GetExtension(name);
      var counter = 1;

      string newFileName;
      do
      {
         newFileName = $"{fileNameWithoutExt} ({counter}){extension}";
         filePath = Path.Combine(folder, newFileName);
         counter++;
      }
      while (File.Exists(filePath));

      return newFileName;
   }

   public static string GetNextAvailableFilePath(string name, string folder)
   {
      var newFileName = GetNextAvailableFileName(name, folder);
      return Path.Combine(folder, newFileName);
   }
}