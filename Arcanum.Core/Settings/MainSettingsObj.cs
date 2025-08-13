using System.Text.Json.Serialization;
using Arcanum.Core.CoreSystems.ErrorSystem;
using Arcanum.Core.GlobalStates;
using Arcanum.Core.GlobalStates.BackingClasses;

namespace Arcanum.Core.Settings;

public class MainSettingsObj
{
   public MainSettingsObj()
   {
      // Initialize any default values here if needed
   }
   
   [IsSubMenu("Key Binds")]
   public UserKeyBinds UserKeyBinds { get; set; } = new();
   
   [JsonIgnore]
   [IsSubMenu("Error Handling")]
   public ErrorDescriptors ErrorDescriptors { get; set; } = ErrorDescriptors.Instance;
   
   
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
