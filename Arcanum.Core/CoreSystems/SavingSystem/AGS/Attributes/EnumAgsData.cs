namespace Arcanum.Core.CoreSystems.SavingSystem.AGS.Attributes;

/// <summary>
/// This attribute is used to annotate enum fields with metadata for AGS serialization.
/// </summary>
/// <param name="key"></param>
/// <param name="isIgnoredInSerialization"></param>
[AttributeUsage(AttributeTargets.Field)]
public class EnumAgsData(string key, bool isIgnoredInSerialization = false) : Attribute
{
   public string Key { get; } = key;
   public bool IsIgnoredInSerialization { get; } = isIgnoredInSerialization;
}