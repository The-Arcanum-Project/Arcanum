using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

namespace Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;

public class ProjectFileDescriptor : IComparable<ProjectFileDescriptor>
{
   public string ModName { get; set; }
   public DataSpace ModPath { get; set; }

   public DataSpace VanillaPath { get; set; }
   public bool IsSubMod { get; set; }
   public List<DataSpace> RequiredMods { get; set; } = [];
   public DateTime LastModified { get; set; }

   /// <summary>
   /// Only meant for serialization purposes.
   /// </summary>
   public ProjectFileDescriptor()
   {
   }

   public ProjectFileDescriptor(string modName, DataSpace modPath, DataSpace vanillaPath)
   {
      ModName = modName;
      ModPath = modPath;
      VanillaPath = vanillaPath;
      LastModified = DateTime.Now;
   }

   public ProjectFileDescriptor(string modName, DataSpace modPath, List<DataSpace> requiredMods, DataSpace vanillaPath)
   {
      ModName = modName;
      ModPath = modPath;
      IsSubMod = requiredMods.Count > 0;
      RequiredMods = requiredMods;
      VanillaPath = vanillaPath;
      LastModified = DateTime.Now;
   }

   public void UpdateLastModified() => LastModified = DateTime.Now;

   public override string ToString()
   {
      return $"{ModName} (SubMod: {IsSubMod}, RequiredMods: {string.Join(", ", RequiredMods)})";
   }

   public int CompareTo(ProjectFileDescriptor? other)
   {
      return other is null
                ? 1
                : LastModified.CompareTo(other.LastModified);
   }

   public override bool Equals(object? obj)
   {
      if (obj is ProjectFileDescriptor other)
      {
         return ModName == other.ModName &&
                IsSubMod == other.IsSubMod &&
                ModPath == other.ModPath &&
                RequiredMods.SequenceEqual(other.RequiredMods);
      }

      return false;
   }

   public override int GetHashCode()
   {
      var hash = new HashCode();
      hash.Add(ModName);
      hash.Add(VanillaPath);
      hash.Add(IsSubMod);
      hash.Add(ModPath);
      foreach (var mod in RequiredMods)
         hash.Add(mod);
      return hash.ToHashCode();
   }

   public static ProjectFileDescriptor GatherFromState()
   {
      // TODO: Implement the logic to gather the current state of the application
      // This method should gather the necessary data from the current state of the application
      // and return a new ProjectFileDescriptor instance.
      // For now, we will return a placeholder instance.
      return new("DefaultMod",
                 new("example", [], DataSpace.AccessType.ReadOnly),
                 [
                    new DataSpace("example", [], DataSpace.AccessType.ReadOnly),
                    new DataSpace("example", [], DataSpace.AccessType.ReadOnly),
                 ],
                 new("example", [], DataSpace.AccessType.ReadOnly));
   }

   public static bool operator <(ProjectFileDescriptor left, ProjectFileDescriptor right)
      => left.LastModified < right.LastModified;

   public static bool operator >(ProjectFileDescriptor left, ProjectFileDescriptor right)
      => left.LastModified > right.LastModified;

   public bool IsValid()
   {
      return !string.IsNullOrEmpty(ModName) &&
             ModPath.IsValid &&
             VanillaPath.IsValid &&
             RequiredMods.All(x => x.IsValid);
   }

   public ImageSource ModThumbnailOrDefault()
   {
      var thumbnailPath = Path.Combine(Path.Combine(ModPath.Path), "thumbnail.png");
      if (!File.Exists(thumbnailPath))
         return new BitmapImage(new("pack://application:,,,/Assets/Logo/ArcanumForeColor.png", UriKind.Absolute));

      return new BitmapImage(new(thumbnailPath, UriKind.Absolute));
   }
}