using System.Text.Json.Serialization;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.CoreSystems.IO.JsonConverters;
using Arcanum.Core.GlobalStates.BackingClasses;
using Arcanum.Core.Settings.SmallSettingsObjects;

namespace Arcanum.Core.Settings;

public class MainSettingsObj
{
   public GeneralNUISettings NUIConfig { get; set; } = new();

   [IsSubMenu("UI")]
   [JsonConverter(typeof(IgnoreDeserializationAndCreateNewConverter<NUISettings>))]
   public NUISettings NUIObjectSettings { get; set; } = new();

   [IsSubMenu("Saving")]
   public AGSSettings AgsSettings { get; set; } = new();

   [IsSubMenu("Map")]
   public MapSettingsObj MapSettings { get; set; } = new();

   public AgsConfig AgsConfig { get; set; } = new();

   [JsonIgnore]
   [IsSubMenu("Error Handling")]
   public ErrorDescriptors ErrorDescriptors { get; set; } = ErrorDescriptors.Instance;

   public ErrorLogExportOptions ErrorLogExportOptions { get; set; } = new();

   [IsSubMenu("Keymap")]
   public UserKeyBinds UserKeyBinds { get; set; } = new();

#if DEBUG
   public DebugConfigSettings DebugConfigSettings { get; set; } = DebugConfig.Settings;
#endif
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class IsSubMenuAttribute : Attribute
{
   public string? Name { get; }

   public IsSubMenuAttribute(string? name = null)
   {
      Name = name;
   }
}