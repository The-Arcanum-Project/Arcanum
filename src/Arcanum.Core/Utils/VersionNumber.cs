namespace Arcanum.Core.Utils;

public static class VersionNumbers
{
   public static VersionNumber V107 => new(1, 0, 7);

   public static VersionNumber Current
   {
      get
      {
         var asm = AppData.AppVersion;
         // Parse the version from the assembly version to a VersionNumber
         var parts = asm.Split('.');
         if (parts.Length < 3 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor) || !int.TryParse(parts[2], out var patch))
            throw new FormatException($"Invalid version format: {asm}");

         return new(major, minor, patch);
      }
   }

   public static string CurrentVersionString => $"{AppData.ProductName} v{Current}";
}

public class VersionNumber
{
   public VersionNumber(int major, int minor, int patch)
   {
      Major = major;
      Minor = minor;
      Patch = patch;
   }

   public VersionNumber(int major, int minor, int patch, SnapShotVersion snapShotVersion) : this(major, minor, patch) => SnapShotVersion = snapShotVersion;

   public int Major { get; }
   public int Minor { get; }
   public int Patch { get; }

   public SnapShotVersion? SnapShotVersion { get; }

   private bool Equals(VersionNumber other) => Major == other.Major &&
                                               Minor == other.Minor &&
                                               Patch == other.Patch &&
                                               Equals(SnapShotVersion, other.SnapShotVersion);

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;
      if (obj.GetType() != GetType())
         return false;

      return Equals((VersionNumber)obj);
   }

   public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, SnapShotVersion);

   public override string ToString()
   {
      var version = $"{Major}.{Minor}.{Patch}";
      if (SnapShotVersion != null)
         version += $"-{SnapShotVersion}";
      return version;
   }

   // Operators for comparing versions
   public static bool operator >(VersionNumber v1, VersionNumber v2)
   {
      if (v1.Major != v2.Major)
         return v1.Major > v2.Major;
      if (v1.Minor != v2.Minor)
         return v1.Minor > v2.Minor;
      if (v1.Patch != v2.Patch)
         return v1.Patch > v2.Patch;

      return v1.SnapShotVersion < v2.SnapShotVersion;
   }

   public static bool operator <(VersionNumber v1, VersionNumber v2) => !(v1 > v2) && v1 != v2;

   public static bool operator ==(VersionNumber v1, VersionNumber v2)
      => v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Patch == v2.Patch && v1.SnapShotVersion == v2.SnapShotVersion;

   public static bool operator !=(VersionNumber v1, VersionNumber v2) => !(v1 == v2);
}

// YYwWWn format, where YY is the year, w is marking weeks with WW being the number of week in the year, and n is the number of snapshot in that week.
public class SnapShotVersion
{
   public SnapShotVersion(int year, int week, int snapshotNumber)
   {
      Year = year;
      Week = week;
      SnapshotNumber = snapshotNumber;
   }

   public int Year { get; }
   public int Week { get; }
   public int SnapshotNumber { get; }
   private bool Equals(SnapShotVersion other) => Year == other.Year && Week == other.Week && SnapshotNumber == other.SnapshotNumber;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;

      return obj.GetType() == GetType() && Equals((SnapShotVersion)obj);
   }

   public override int GetHashCode() => HashCode.Combine(Year, Week, SnapshotNumber);

   private static string NumberToChar(int number)
   {
      return number switch
      {
         < 0 or > 35 => throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 0 and 35."),
         < 10 => number.ToString(),
         _ => ((char)('A' + (number - 10))).ToString(),
      };
   }

   public override string ToString() => $"{Year % 100:00}{Week:00}{NumberToChar(SnapshotNumber)}";

   public static bool operator <(SnapShotVersion? s1, SnapShotVersion? s2)
   {
      switch (s1)
      {
         case null when s2 is null:
            return false;
         case null:
            return true;
      }

      if (s2 is null)
         return false;
      if (s1.Year != s2.Year)
         return s1.Year < s2.Year;
      if (s1.Week != s2.Week)
         return s1.Week < s2.Week;

      return s1.SnapshotNumber < s2.SnapshotNumber;
   }

   public static bool operator >(SnapShotVersion? s1, SnapShotVersion? s2) => !(s1 < s2) && s1 != s2;

   public static bool operator ==(SnapShotVersion? s1, SnapShotVersion? s2)
   {
      if (s1 is null && s2 is null)
         return true;
      if (s1 is null || s2 is null)
         return false;

      return s1.Year == s2.Year && s1.Week == s2.Week && s1.SnapshotNumber == s2.SnapshotNumber;
   }

   public static bool operator !=(SnapShotVersion? s1, SnapShotVersion? s2) => !(s1 == s2);
}