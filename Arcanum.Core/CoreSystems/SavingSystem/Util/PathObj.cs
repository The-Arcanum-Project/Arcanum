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
    public static readonly PathObj Empty = new PathObj([], string.Empty, DataSpace.Empty);

    public readonly string[] LocalPath = localPath;
    public string Filename { get; } = filename;
    public readonly DataSpace DataSpace = dataSpace;
    
    
    /// <summary>
    /// The full path as a string, combining the root position, local path, and filename.
    /// </summary>
    public string FullPath => Path.Combine(Path.Combine(DataSpace.Path), Path.Combine(LocalPath), Filename);
}