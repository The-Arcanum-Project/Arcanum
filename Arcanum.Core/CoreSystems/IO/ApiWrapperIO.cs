using System.Drawing.Imaging;
using System.Text;
using Arcanum.API.Core.IO;
using Arcanum.API.UtilServices;

namespace Arcanum.Core.CoreSystems.IO;

public class APIWrapperIO : IFileOperations
{
   public void Unload()
   {
   }

   // We have no internal state to verify in this service, so we return Ok state.
   public IService.ServiceState VerifyState() => IService.ServiceState.Ok;
   public string GetArcanumDataPath { get; } = IO.GetArcanumDataPath;

   public string? SelectFolder(string startPath, string defaultFileName = "Select Folder")
      => IO.SelectFolder(startPath, defaultFileName);

   public string? SelectFile(string startFolder, string filterText) => IO.SelectFile(startFolder, filterText);

   public string? ReadAllText(string path, Encoding encoding) => IO.ReadAllText(path, encoding);

   public string[]? ReadAllLines(string path, Encoding encoding) => IO.ReadAllLines(path, encoding);

   public string? ReadAllTextAnsi(string path) => IO.ReadAllTextAnsi(path);

   public string[]? ReadAllLinesAnsi(string path) => IO.ReadAllLinesAnsi(path);

   public string? ReadAllTextUtf8(string path) => IO.ReadAllTextUtf8(path);

   public string[]? ReadAllLinesUtf8(string path) => IO.ReadAllLinesUtf8(path);

   public string? ReadAllTextUtf8WithBom(string path) => IO.ReadAllTextUtf8WithBom(path);

   public string[]? ReadAllLinesUtf8WithBom(string path) => IO.ReadAllLinesUtf8WithBom(path);

   public bool WriteAllText(string path, string data, Encoding encoding, bool append = false)
      => IO.WriteAllText(path, data, encoding, append);

   public bool WriteAllTextAnsi(string path, string data, bool append = false)
      => IO.WriteAllTextAnsi(path, data, append);

   public bool WriteAllTextUtf8(string path, string data, bool append = false)
      => IO.WriteAllTextUtf8(path, data, append);

   public bool WriteAllTextUtf8WithBom(string path, string data, bool append = false)
      => IO.WriteAllTextUtf8WithBom(path, data, append);

   public bool EnsureDirectoryExists(string directoryPath) => IO.EnsureDirectoryExists(directoryPath);

   public bool EnsureFileDirectoryExists(string filePath) => IO.EnsureFileDirectoryExists(filePath);

   public bool FileExists(string path) => IO.FileExists(path);

   public bool DirectoryExists(string path) => IO.DirectoryExists(path);

   public bool SaveBitmap(string path, Bitmap bmp, ImageFormat format) => IO.SaveBitmap(path, bmp, format);

   public Task<string?> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
      => IO.ReadAllTextAsync(path, encoding, cancellationToken);

   public Task<string[]?> ReadAllLinesAsync(string path,
                                            Encoding encoding,
                                            CancellationToken cancellationToken = default)
      => IO.ReadAllLinesAsync(path, encoding, cancellationToken);

   public Task<string?> ReadAllTextAnsiAsync(string path, CancellationToken cancellationToken = default)
      => IO.ReadAllTextAnsiAsync(path, cancellationToken);

   public Task<string[]?> ReadAllLinesAnsiAsync(string path, CancellationToken cancellationToken = default)
      => IO.ReadAllLinesAnsiAsync(path, cancellationToken);

   public Task<string?> ReadAllTextUtf8Async(string path, CancellationToken cancellationToken = default)
      => IO.ReadAllTextUtf8Async(path, cancellationToken);

   public Task<string[]?> ReadAllLinesUtf8Async(string path, CancellationToken cancellationToken = default)
      => IO.ReadAllLinesUtf8Async(path, cancellationToken);

   public Task<string?> ReadAllTextUtf8WithBomAsync(string path, CancellationToken cancellationToken = default)
      => IO.ReadAllTextUtf8WithBomAsync(path, cancellationToken);

   public Task<string[]?> ReadAllLinesUtf8WithBomAsync(string path, CancellationToken cancellationToken = default)
      => IO.ReadAllLinesUtf8WithBomAsync(path, cancellationToken);

   public Task<bool> WriteAllTextAsync(string path,
                                       string data,
                                       Encoding encoding,
                                       bool append = false,
                                       CancellationToken cancellationToken = default)
      => IO.WriteAllTextAsync(path, data, encoding, append, cancellationToken);

   public Task<bool> WriteAllTextAnsiAsync(string path,
                                           string data,
                                           bool append = false,
                                           CancellationToken cancellationToken = default)
      => IO.WriteAllTextAnsiAsync(path, data, append, cancellationToken);

   public Task<bool> WriteAllTextUtf8Async(string path,
                                           string data,
                                           bool append = false,
                                           CancellationToken cancellationToken = default)
      => IO.WriteAllTextUtf8Async(path, data, append, cancellationToken);

   public Task<bool> WriteAllTextUtf8WithBomAsync(string path,
                                                  string data,
                                                  bool append = false,
                                                  CancellationToken cancellationToken = default)
      => IO.WriteAllTextUtf8WithBomAsync(path, data, append, cancellationToken);

   public Task<bool> SaveBitmapAsync(string path,
                                     Bitmap bmp,
                                     ImageFormat format,
                                     CancellationToken cancellationToken = default)
      => IO.SaveBitmapAsync(path, bmp, format, cancellationToken);
}