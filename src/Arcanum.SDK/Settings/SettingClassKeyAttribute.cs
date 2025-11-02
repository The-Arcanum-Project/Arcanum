namespace Arcanum.API.Settings;

/// <summary>
/// Specifies a unique, stable string key for a class implementing IPluginSetting.
/// This key is used for polymorphic serialization and deserialization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PluginSettingKeyAttribute : Attribute
{
   public string Key { get; }

   /// <summary>
   /// Initializes a new instance of the PluginSettingKeyAttribute.
   /// </summary>
   /// <param name="key">A unique, stable key. It should not be changed after being deployed.</param>
   public PluginSettingKeyAttribute(string key)
   {
      if (string.IsNullOrWhiteSpace(key))
         throw new ArgumentNullException(nameof(key), "Plugin setting key cannot be null or whitespace.");

      Key = key;
   }
}