using Arcanum.API;
using Arcanum.API.UtilServices;

namespace Arcanum.PluginHost.PluginServices;

public interface IPluginInfoService : IService
{
   IPlugin? GetPluginByName(string name);
   public IPlugin? GetPluginByGuid(Guid id);
   bool IsPluginLoaded(string name);
   IPluginMetadata? GetMetadata(string name);
   Version? GetPluginVersion(string name);
}