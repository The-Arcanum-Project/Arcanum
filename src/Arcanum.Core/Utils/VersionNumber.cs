#region

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#endregion

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Arcanum.Core.Utils;

public static class VersionNumbers
{
   public static readonly VersionNumber V107 = new(1, 0, 7);
   public static readonly VersionNumber V1072 = new(1, 0, 7, 2);

   public static VersionNumber Current => AppData.AppVersion;

   public static string CurrentVersionString => $"{AppData.ProductName} {Current}";
   public static VersionNumber Undefined => new(-1, -1, -1, -1);
}

[TypeConverter(typeof(VersionNumberConverter))]
public class VersionNumber : IComparable<VersionNumber>
{
   // For parsers
   public VersionNumber()
   {
   }

   public VersionNumber(int major, int minor, int patch, int hotfix = 0)
   {
      Major = major;
      Minor = minor;
      Patch = patch;
      Hotfix = hotfix;
   }

   public VersionNumber(int major, int minor, int patch, int hotfix, SnapShotVersion snapShotVersion) : this(major,
      minor,
      patch,
      hotfix) => SnapShotVersion = snapShotVersion;

   public int Major { get; set; }
   public int Minor { get; set; }
   public int Patch { get; set; }
   public int Hotfix { get; set; }

   public SnapShotVersion? SnapShotVersion { get; set; }

   public string WithVPrefix => $"{this}";

   public int CompareTo(VersionNumber? other)
   {
      if (other is null)
         return 1;
      if (Major != other.Major)
         return Major.CompareTo(other.Major);
      if (Minor != other.Minor)
         return Minor.CompareTo(other.Minor);
      if (Patch != other.Patch)
         return Patch.CompareTo(other.Patch);
      if (Hotfix != other.Hotfix)
         return Hotfix.CompareTo(other.Hotfix);

      if (SnapShotVersion == null && other.SnapShotVersion == null)
         return 0;
      if (SnapShotVersion == null)
         return 1; // Non-snapshot versions are considered greater than snapshot versions
      if (other.SnapShotVersion == null)
         return -1;

      return SnapShotVersion.CompareTo(other.SnapShotVersion);
   }

   public static bool FromTag(string releaseTag, [MaybeNullWhen(false)] out VersionNumber version)
   {
      var versionPart = releaseTag.StartsWith('v') ? releaseTag[1..] : releaseTag;
      var split = versionPart.Split('-');
      version = null;

      if (split.Length == 0)
         return false;

      var versionNumbers = split[0].Split('.');

      // Sequentially parse the version numbers, allowing for missing numbers (e.g., "1.0" would be parsed as major=1, minor=0, patch=0)
      if (!int.TryParse(versionNumbers[0], out var major))
         return false;

      var minor = versionNumbers.Length > 1 && int.TryParse(versionNumbers[1], out var minorParsed) ? minorParsed : 0;
      var patch = versionNumbers.Length > 2 && int.TryParse(versionNumbers[2], out var patchParsed) ? patchParsed : 0;
      var hotfix = versionNumbers.Length > 3 && int.TryParse(versionNumbers[3], out var hotfixParsed)
                      ? hotfixParsed
                      : 0;

      version = new(major, minor, patch, hotfix);

      return true;
   }

   private bool Equals(VersionNumber other) => Major == other.Major &&
                                               Minor == other.Minor &&
                                               Patch == other.Patch &&
                                               Hotfix == other.Hotfix &&
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

   public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Hotfix, SnapShotVersion);

   public override string ToString()
   {
      var hotfixPart = Hotfix > 0 ? $".{Hotfix}" : string.Empty;
      var version = $"v{Major}.{Minor}.{Patch}{hotfixPart}";
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
      if (v1.Hotfix != v2.Hotfix)
         return v1.Hotfix > v2.Hotfix;

      return v1.SnapShotVersion < v2.SnapShotVersion;
   }

   public static bool operator <(VersionNumber v1, VersionNumber v2) => !(v1 > v2) && v1 != v2;

   public static bool operator ==(VersionNumber v1, VersionNumber v2) => v1.Major == v2.Major &&
                                                                         v1.Minor == v2.Minor &&
                                                                         v1.Patch == v2.Patch &&
                                                                         v1.Hotfix == v2.Hotfix &&
                                                                         v1.SnapShotVersion == v2.SnapShotVersion;

   public static bool operator !=(VersionNumber v1, VersionNumber v2) => !(v1 == v2);
}

// YYwWWn format, where YY is the year, w is marking weeks with WW being the number of week in the year, and n is the number of snapshot in that week.
[TypeConverter(typeof(SnapShotVersionConverter))]
public class SnapShotVersion : IComparable<SnapShotVersion>
{
   public SnapShotVersion(int year, int week, int snapshotNumber)
   {
      Year = year;
      Week = week;
      SnapshotNumber = snapshotNumber;
   }

   // For parsers
   public SnapShotVersion()
   {
   }

   public int Year { get; set; }
   public int Week { get; set; }
   public int SnapshotNumber { get; set; }

   public int CompareTo(SnapShotVersion? other)
   {
      if (other is null)
         return 1;
      if (Year != other.Year)
         return Year.CompareTo(other.Year);
      if (Week != other.Week)
         return Week.CompareTo(other.Week);

      return SnapshotNumber.CompareTo(other.SnapshotNumber);
   }

   private bool Equals(SnapShotVersion other) => Year == other.Year && Week == other.Week && SnapshotNumber == other.SnapshotNumber;

   public override bool Equals(object? obj)
   {
      if (obj is null)
         return false;
      if (ReferenceEquals(this, obj))
         return true;

      return obj.GetType() == GetType() && Equals((SnapShotVersion)obj);
   }

   public static bool TryParse(string input, [MaybeNullWhen(false)] out SnapShotVersion result)
   {
      result = null;
      if (string.IsNullOrWhiteSpace(input) || input.Length < 5)
         return false;

      if (int.TryParse(input[..2], out var yy) &&
          int.TryParse(input[2..4], out var ww))
      {
         var n = char.ToUpper(input[4]);
         var snapshotNum = char.IsDigit(n) ? n - '0' : n - 'A' + 10;

         result = new(2000 + yy, ww, snapshotNum);
         return true;
      }

      return false;
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

public class VersionNumberConverter : TypeConverter
{
   public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
      => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

   public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
   {
      if (value is string s)
      {
         if (VersionNumber.FromTag(s, out var version))
            return version;

         throw new FormatException($"Invalid VersionNumber format: {s}");
      }

      return base.ConvertFrom(context, culture, value);
   }
}

public class SnapShotVersionConverter : TypeConverter
{
   public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
      => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

   public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
   {
      if (value is string s)
      {
         if (SnapShotVersion.TryParse(s, out var version))
            return version;

         throw new FormatException($"Invalid SnapShotVersion format: {s}");
      }

      return base.ConvertFrom(context, culture, value);
   }
}