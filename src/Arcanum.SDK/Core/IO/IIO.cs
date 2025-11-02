using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Arcanum.API.UtilServices;

namespace Arcanum.API.Core.IO;

/// <summary>
/// Defines operations for file system interactions, dialogs, and image handling.
/// </summary>
public interface IFileOperations : IService
{
   /// <summary>
   /// Gets the file system path where Arcanum data is stored.
   /// </summary>
   /// <value>
   /// A string representing the directory path to the Arcanum data.
   /// </value>
   string GetArcanumDataPath { get; }

   // --- Dialogs ---
   /// <summary>
   /// Opens a folder selection dialog starting from a specified path.
   /// </summary>
   /// <param name="startPath">The initial folder path to display in the dialog.</param>
   /// <param name="defaultFileName">A default name to appear in the file name field, often used as a trick to pre-select or suggest a context for folder selection.</param>
   /// <returns>The selected folder path, or null if no folder was selected or the dialog was cancelled.</returns>
   string? SelectFolder(string startPath, string defaultFileName = "Select Folder");

   /// <summary>
   /// Opens a file selection dialog starting from a specified folder and filters the visible files.
   /// </summary>
   /// <param name="startFolder">The initial folder path to display in the dialog.</param>
   /// <param name="filterText">The filter string to determine the types of files displayed (e.g., "Text files (*.txt)|*.txt|All files (*.*)|*.*").</param>
   /// <returns>The path of the selected file, or null if no file was selected or the dialog was cancelled.</returns>
   string? SelectFile(string startFolder, string filterText);

   // --- File Read Operations ---
   /// <summary>
   /// Reads the entire content of a file using the specified encoding.
   /// </summary>
   /// <param name="path">The file path to read from.</param>
   /// <param name="encoding">The encoding to use.</param>
   /// <returns>The file content as a string or null if the file doesn't exist or an error occurs.</returns>
   string? ReadAllText(string path, Encoding encoding);

   /// <summary>
   /// Reads all lines from a file using the specified encoding.
   /// </summary>
   /// <param name="path">The file path to read from.</param>
   /// <param name="encoding">The encoding to use.</param>
   /// <returns>An array of lines, or null if the file doesn't exist or an error occurs.</returns>
   string[]? ReadAllLines(string path, Encoding encoding);

   string? ReadAllTextAnsi(string path);
   string[]? ReadAllLinesAnsi(string path);
   string? ReadAllTextUtf8(string path); // UTF-8 without BOM
   string[]? ReadAllLinesUtf8(string path); // UTF-8 without BOM
   string? ReadAllTextUtf8WithBom(string path);
   string[]? ReadAllLinesUtf8WithBom(string path);

   // --- File Write Operations ---
   /// <summary>
   /// Writes text to a file using the specified encoding.
   /// </summary>
   /// <param name="path">The file path to write to.</param>
   /// <param name="data">The string data to write.</param>
   /// <param name="encoding">The encoding to use.</param>
   /// <param name="append">True to append to the file; false to overwrite.</param>
   /// <returns>True if successful, false otherwise.</returns>
   bool WriteAllText(string path, string data, Encoding encoding, bool append = false);

   bool WriteAllTextAnsi(string path, string data, bool append = false);
   bool WriteAllTextUtf8(string path, string data, bool append = false); // UTF-8 without BOM
   bool WriteAllTextUtf8WithBom(string path, string data, bool append = false);

   // --- Directory Operations ---
   /// <summary>
   /// Ensures that the specified directory path exists, creating it if necessary.
   /// </summary>
   /// <param name="directoryPath">The path of the directory.</param>
   /// <returns>True if the directory exists or was successfully created, false otherwise.</returns>
   bool EnsureDirectoryExists(string directoryPath);

   /// <summary>
   /// Ensures that the parent directory of the specified file path exists, creating it if necessary.
   /// </summary>
   /// <param name="filePath">The path of the file whose parent directory needs to exist.</param>
   /// <returns>True if the parent directory exists or was successfully created, false otherwise.</returns>
   bool EnsureFileDirectoryExists(string filePath);

   // --- File/Directory Checks ---
   bool FileExists(string path);
   bool DirectoryExists(string path);

   // --- Image Operations ---
   /// <summary>
   /// Saves a Bitmap to a file with the specified image format.
   /// </summary>
   /// <param name="path">The path to save the image to.</param>
   /// <param name="bmp">The Bitmap to save.</param>
   /// <param name="format">The image format (e.g., ImageFormat.Png, ImageFormat.Jpeg).</param>
   /// <returns>True if successful, false otherwise.</returns>
   bool SaveBitmap(string path, Bitmap bmp, ImageFormat format);

   // --- Async Operations ---
   Task<string?> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default);
   Task<string[]?> ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken = default);
   Task<string?> ReadAllTextAnsiAsync(string path, CancellationToken cancellationToken = default);
   Task<string[]?> ReadAllLinesAnsiAsync(string path, CancellationToken cancellationToken = default);
   Task<string?> ReadAllTextUtf8Async(string path, CancellationToken cancellationToken = default);
   Task<string[]?> ReadAllLinesUtf8Async(string path, CancellationToken cancellationToken = default);
   Task<string?> ReadAllTextUtf8WithBomAsync(string path, CancellationToken cancellationToken = default);
   Task<string[]?> ReadAllLinesUtf8WithBomAsync(string path, CancellationToken cancellationToken = default);

   Task<bool> WriteAllTextAsync(string path,
                                string data,
                                Encoding encoding,
                                bool append = false,
                                CancellationToken cancellationToken = default);

   Task<bool> WriteAllTextAnsiAsync(string path,
                                    string data,
                                    bool append = false,
                                    CancellationToken cancellationToken = default);

   Task<bool> WriteAllTextUtf8Async(string path,
                                    string data,
                                    bool append = false,
                                    CancellationToken cancellationToken = default);

   Task<bool> WriteAllTextUtf8WithBomAsync(string path,
                                           string data,
                                           bool append = false,
                                           CancellationToken cancellationToken = default);

   Task<bool> SaveBitmapAsync(string path,
                              Bitmap bmp,
                              ImageFormat format,
                              CancellationToken cancellationToken = default);
}