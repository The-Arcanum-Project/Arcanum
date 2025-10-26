using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Arcanum.Core.CoreSystems.History;
using Arcanum.Core.CoreSystems.History.HistoryDto;
using Arcanum.Core.CoreSystems.Parsing.DocumentsLoading;
using Arcanum.Core.CoreSystems.SavingSystem.Util;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Arcanum.Core.CoreSystems.ProjectFileUtil.Mod;

public class ProjectFileDescriptor : IComparable<ProjectFileDescriptor>
{
   public string ModName { get; set; } = null!;
   public DataSpace ModPath { get; set; } = null!;
   public DataSpace VanillaPath { get; set; } = null!;
   public bool IsSubMod { get; set; }
   public List<DataSpace> RequiredMods { get; set; } = [];
   public DateTime LastModified { get; set; }
   public HistoryNodeDto? History { get; set; }

   /// <summary>
   /// Only meant for serialization purposes.
   /// </summary>
   [JsonConstructor]
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

   public void UpdateForClose()
   {
      LastModified = DateTime.Now;
      History = HistoryDtoManager.GetHistoryAsDto();
   }

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
         return ModName == other.ModName &&
                IsSubMod == other.IsSubMod &&
                ModPath == other.ModPath &&
                RequiredMods.SequenceEqual(other.RequiredMods);

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
      var metadata = ExistingModsLoader.ParseModMetadata(ModPath.FullPath);
      var thumbnailPath = Path.Combine(ModPath.FullPath, ".metadata", metadata?.ThumbnailPath ?? string.Empty);
      return !File.Exists(thumbnailPath)
                ? new(new("/Arcanum_UI;component/Assets/Logo/ArcanumForeColor.png",
                          UriKind.RelativeOrAbsolute))
                : new BitmapImage(new(thumbnailPath, UriKind.Absolute));
   }
}