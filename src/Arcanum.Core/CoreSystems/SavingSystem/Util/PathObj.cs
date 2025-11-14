using System.IO;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

/// <summary>
/// Represents a Path separated into a root position, local path, and filename.
/// </summary>
/// <param name="localPath">
///     The local path components as an array of strings.
/// </param>
/// <param name="filename">
///     The filename as a string including the file extension.
/// </param>
/// <param name="dataSpace">
///     The root position indicating whether the path is relative to the vanilla game files or a mod.
/// </param>
public class PathObj(string[] localPath, string filename, DataSpace dataSpace)
{
   public static readonly PathObj Empty = new([], string.Empty, DataSpace.Empty);

   public readonly string[] LocalPath = localPath;
   public string Filename { get; set; } = filename;
   public DataSpace DataSpace { get; private set; } = dataSpace;

   public void AddSearchTerms(ICollection<string> terms)
   {
      foreach (var term in LocalPath)
         terms.Add(term);
      terms.Add(Filename);
      terms.Add(FilenameWithoutExtension);
   }

   public void MoveToMod()
   {
      // We tell the FileStateManager to stop watching this path first and then re-register it under the new DataSpace.
      RegisterWatcher();
      DataSpace = FileManager.ModDataSpace;
      UnregisterWatcher();
   }

   public void RegisterWatcher() => FileWatcher.FileStateManager.RegisterPath(this);
   public void UnregisterWatcher() => FileWatcher.FileStateManager.UnregisterPath(this);

   public string FilenameWithoutExtension => Path.GetFileNameWithoutExtension(Filename);

   public string RelativePath => Path.Combine(Path.Combine(LocalPath), Filename);
   /// <summary>
   /// The full path as a string, combining the root position, local path, and filename.
   /// </summary>
   public string FullPath => Path.Combine(Path.Combine(DataSpace.Path), Path.Combine(LocalPath), Filename);

   public string FullPathWithoutFilename => Path.Combine(Path.Combine(DataSpace.Path), Path.Combine(LocalPath));
}