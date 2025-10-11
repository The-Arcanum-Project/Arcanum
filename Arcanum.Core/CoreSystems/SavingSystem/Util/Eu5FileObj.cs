using Arcanum.Core.CoreSystems.SavingSystem.FileWatcher;
using Arcanum.Core.GameObjects.BaseTypes;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

public class Eu5FileObj
{
   public Eu5FileObj(PathObj path, FileDescriptor descriptor)
   {
      Descriptor = descriptor;
      Path = path;
   }

   public FileDescriptor Descriptor { get; }
   public PathObj Path { get; }
   public HashSet<IEu5Object> ObjectsInFile { get; } = [];
   public static Eu5FileObj Empty { get; } = new(PathObj.Empty, FileDescriptor.Empty);
   public byte[] Checksum { get; private set; } = [];

   public void GenerateChecksum()
   {
      Checksum = Task.Run(() => FileStateManager.CalculateSha256(Path.FullPath)).Result;
   }

   private bool Equals(Eu5FileObj other) => Descriptor.Equals(other.Descriptor) && Path.Equals(other.Path);
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
   public static bool operator ==(Eu5FileObj? left, Eu5FileObj? right) => Equals(left, right);
   public static bool operator !=(Eu5FileObj? left, Eu5FileObj? right) => !Equals(left, right);
}