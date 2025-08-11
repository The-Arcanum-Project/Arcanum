namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public abstract class FileObj(PathObj path, FileDescriptor descriptor)
{
   public FileDescriptor Descriptor { get; set; } = descriptor;

   public readonly PathObj Path = path;

   public abstract IEnumerable<ISaveable> GetSaveables();
}

public class DummyFileObj : FileObj
{
   public DummyFileObj(PathObj path, FileDescriptor descriptor) : base(path,
    descriptor)
   {
   }

   public override IEnumerable<ISaveable> GetSaveables()
   {
      return [];
   }
}

public class FileObj<T>(PathObj path, FileDescriptor descriptor)
   : FileObj(path, descriptor)
   where T : ISaveable
{
   public readonly List<T> Saveables = [];

   public override IEnumerable<ISaveable> GetSaveables()
   {
      return Saveables.Cast<ISaveable>();
   }
}