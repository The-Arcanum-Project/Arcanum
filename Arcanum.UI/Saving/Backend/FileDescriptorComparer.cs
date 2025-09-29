using Arcanum.Core.CoreSystems.Common;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.UI.Saving.Backend;

public class FileDescriptorComparer : IComparer<FileDescriptor>
{
    public int Compare(FileDescriptor? x, FileDescriptor? y)
    {
        return NaturalStringComparer.Compare(x?.Name, y?.Name);
    }
}