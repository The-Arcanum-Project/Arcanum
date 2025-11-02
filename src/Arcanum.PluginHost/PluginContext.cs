using System.Reflection;
using System.Runtime.Loader;

namespace Arcanum.PluginHost;

public class PluginLoadContext : AssemblyLoadContext
{
   private readonly AssemblyDependencyResolver _resolver;

   public PluginLoadContext(string pluginPath) : base(isCollectible: true) => _resolver = new(pluginPath);

   protected override Assembly? Load(AssemblyName name)
   {
      var path = _resolver.ResolveAssemblyToPath(name);
      return path != null ? LoadFromAssemblyPath(path) : null;
   }
}