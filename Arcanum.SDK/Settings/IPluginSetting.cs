namespace Arcanum.API.Settings;

/// <summary>
/// Represents a plugin setting interface which marks objects as plugin settings.
/// To have plugins properly visible in the settings UI, make sure to implement the
/// following attributes on each property:
/// - [Category("General")]
/// - [Description("Your API Key for the service.")]
/// - [DefaultValue(3)]
/// </summary>
public interface IPluginSetting
{
   /// <summary>
   /// Gets the unique identifier for the owner associated with the plugin setting.
   /// </summary>
   public Guid OwnerGuid { get; }
}