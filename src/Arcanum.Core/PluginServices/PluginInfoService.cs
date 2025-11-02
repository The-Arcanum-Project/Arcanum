using Arcanum.API;
using Arcanum.API.UtilServices;
using Arcanum.PluginHost.PluginServices;

namespace Arcanum.Core.PluginServices;

public class PluginInfoService(PluginManager manager) : IPluginInfoService
{
   private readonly PluginManager _pluginManager =
      manager ?? throw new ArgumentNullException(nameof(manager), "PluginManager cannot be null.");

   public IPlugin? GetPluginByName(string name)
      => _pluginManager.LoadedPlugins.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

   public IPlugin? GetPluginByGuid(Guid id) => _pluginManager[id];

   public bool IsPluginLoaded(string name) => GetPluginByName(name) is { IsActive: true };

   public IPluginMetadata? GetMetadata(string name) => GetPluginByName(name);

   public Version? GetPluginVersion(string name) => GetPluginByName(name)?.PluginVersion;

   public void Unload()
   {
   }

   // There is nothing to verify in this service, so we return Ok state.
   public IService.ServiceState VerifyState() => IService.ServiceState.Ok;
}