using Arcanum.API.Settings;

namespace Arcanum.PluginHost.Settings;

internal class PluginSetting<T> : IPluginSetting
{
   public string Key { get; }
   public Guid OwnerGuid { get; }
   public string? Description { get; }
   public bool VisibleToUser { get; }
   public Type ValueType => typeof(T);

   private T _value;
   public object Value
   {
      get => _value!;
      set
      {
         _value = (T)value;
         LastModified = DateTime.UtcNow;
      }
   }

   public DateTime LastModified { get; private set; }

   public PluginSetting(string key, T defaultValue, Guid ownerGuid, string? description, bool visible)
   {
      Key = key;
      OwnerGuid = ownerGuid;
      Description = description;
      VisibleToUser = visible;
      _value = defaultValue;
      LastModified = DateTime.UtcNow;
   }
}