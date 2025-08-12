namespace Arcanum.Core.CoreSystems.SavingSystem.Util.InformationStructs;

public readonly struct FileInformation(string fileName,
    bool allowsOverwrite, FileDescriptor descriptor)
{
    public static FileInformation Empty { get; }= new("EmptyFile", false, FileDescriptor.Dummy);
    
    /// <summary>
    /// The default file name for the saveable
    /// </summary>
    public readonly string FileName = fileName;

    /// <summary>
    /// If the filename is allowed to be overwritten by the user.
    /// Especially when creating a new instance of the saveable, since it might be forced to have a specific name.
    /// </summary>
    public readonly bool AllowsOverwrite = allowsOverwrite;
    
    
    public readonly FileDescriptor Descriptor = descriptor;
    
}