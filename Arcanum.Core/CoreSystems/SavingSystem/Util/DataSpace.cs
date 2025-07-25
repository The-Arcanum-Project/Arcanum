using System.IO;

namespace Arcanum.Core.CoreSystems.SavingSystem.Util;

/// <summary>
/// Defines the root of a file structure.
/// Might either be a read-only directory or a read-write directory.
/// Base-Mods and Vanilla are implemented as read-only directories.
/// The current Mod is implemented as a read-write directory.
/// </summary>
public class DataSpace
{
   public DataSpace()
   {
      Access = AccessType.ReadOnly;
      Path = [];
      Name = string.Empty;
   }
   public enum AccessType
   {
      ReadOnly,
      ReadWrite,
   }

   public static readonly DataSpace Empty = new(string.Empty, ["ThisShouldNotExist"], AccessType.ReadOnly);

   public AccessType Access { get; set; }
   public string[] Path { get; set; }
   public string Name { get; set; }

   /// <summary>
   /// Defines the root of a file structure.
   /// Might either be a read-only directory or a read-write directory.
   /// Base-Mods and Vanilla are implemented as read-only directories.
   /// The current Mod is implemented as a read-write directory.
   /// </summary>
   public DataSpace(string name, string[] path, AccessType access)
   {
      Access = access;
      Path = path;
      Name = name;
   }

   [System.Text.Json.Serialization.JsonIgnore]
   public string FullPath => System.IO.Path.Combine(Path);
   
   [System.Text.Json.Serialization.JsonIgnore]
   public bool IsValid => Path.Length != 0 &&
                          !Path.Any(string.IsNullOrEmpty) &&
                          Directory.Exists(System.IO.Path.Combine(Path));
}