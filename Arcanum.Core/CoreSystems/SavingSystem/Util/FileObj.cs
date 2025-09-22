using Arcanum.Core.CoreSystems.SavingSystem.AGS;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public abstract class FileObj(PathObj path, FileDescriptor descriptor) : IEmpty<FileObj>
{
   public FileDescriptor Descriptor { get; } = descriptor;

   public PathObj Path { get; } = path;

   public abstract IEnumerable<IEu5Object> GetSaveables();
   public static FileObj Empty { get; } = new DefaultFileObject(PathObj.Empty, FileDescriptor.Empty);

   public override string ToString() => $"{Descriptor} @ {Path}";

   protected bool Equals(FileObj other) => Descriptor.Equals(other.Descriptor) && Path.Equals(other.Path);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((FileObj)obj);
   }

   public override int GetHashCode() => HashCode.Combine(Descriptor, Path);
}

public class DefaultFileObject(PathObj path, FileDescriptor descriptor) : FileObj(path,
    descriptor)
{
   public override IEnumerable<IEu5Object> GetSaveables()
   {
      return [];
   }
}

public class Eu5FileObj<T>(PathObj path, FileDescriptor descriptor)
   : FileObj(path, descriptor) where T : IEu5Object<T>, new()
{
   public override IEnumerable<IEu5Object> GetSaveables()
   {
      return GetEu5Objects().Cast<IEu5Object>();
   }

   /// <summary>
   /// Returns all EU5 objects that originate from this file.
   /// </summary>
   /// <returns></returns>
   public IEnumerable<T> GetEu5Objects()
   {
      List<T> objects = [];
      foreach (var obj in T.GetGlobalItems().Values)
         if (ReferenceEquals(obj.Source, this))
            objects.Add(obj);

      return objects;
   }
}

public class FileObj<T>(PathObj path, FileDescriptor descriptor)
   : FileObj(path, descriptor)
   where T : ISaveable
{
   public readonly List<T> Saveables = [];

   public override IEnumerable<IEu5Object> GetSaveables()
   {
      return Saveables.Cast<IEu5Object>();
   }
}