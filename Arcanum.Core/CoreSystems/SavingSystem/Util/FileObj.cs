using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class Eu5FileObj(PathObj path, FileDescriptor descriptor)
{
   public FileDescriptor Descriptor { get; } = descriptor;
   public PathObj Path { get; } = path;

   public HashSet<IEu5Object> ObjectsInFile { get; } = [];

   public static Eu5FileObj Empty { get; } = new(PathObj.Empty, FileDescriptor.Empty);
   protected bool Equals(Eu5FileObj other) => Descriptor.Equals(other.Descriptor) && Path.Equals(other.Path);

   public override string ToString() => $"{Descriptor} @ {Path}";

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((Eu5FileObj)obj);
   }

   public override int GetHashCode() => HashCode.Combine(Descriptor, Path);
}