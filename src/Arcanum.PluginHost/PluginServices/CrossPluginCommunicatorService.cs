using Arcanum.API;
using Arcanum.API.CrossPluginServices;
using Arcanum.API.UtilServices;

namespace Arcanum.PluginHost.PluginServices;

public class CrossPluginCommunicatorService : ICrossPluginCommunicator
{
   // Outer Key: Guid of the publisher
   // Inner Key: Type of the service interface (e.g., typeof(IMySpecialApi))
   // Value: The actual serviceInstance
   private readonly Dictionary<Guid, Dictionary<Type, object>> _publishedServices = new();
   private readonly object _lock = new();

   private readonly IPluginHost _host;
   private readonly IPluginInfoService _infoService;

   private const string CROSS_PLUGIN_COM_NAME = "CPlgComS"; // max 8 chars

   public CrossPluginCommunicatorService(IPluginHost host, IPluginInfoService infoService)
   {
      _host = host ?? throw new ArgumentNullException(nameof(host));
      _infoService = infoService ?? throw new ArgumentNullException(nameof(infoService));
   }

   public void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info)
      => _host.Log(CROSS_PLUGIN_COM_NAME, message, verbosity);

   public bool PublishService<TInterface>(IPlugin publishingPlugin, TInterface serviceInstance) where TInterface : class
   {
      ArgumentNullException.ThrowIfNull(publishingPlugin);
      ArgumentNullException.ThrowIfNull(serviceInstance);

      lock (_lock)
      {
         if (!_publishedServices.TryGetValue(publishingPlugin.Guid, out var servicesByThisPlugin))
         {
            servicesByThisPlugin = new();
            _publishedServices[publishingPlugin.Guid] = servicesByThisPlugin;
         }

         if (servicesByThisPlugin.ContainsKey(typeof(TInterface)))
         {
            Log($"Plugin '{publishingPlugin.Name}' already published a service of type '{typeof(TInterface).FullName}'. Publication failed.",
                LoggingVerbosity.Warning);
            return false; // Already published this type of service
         }

         servicesByThisPlugin[typeof(TInterface)] = serviceInstance;
         Log($"Plugin '{publishingPlugin.Name}' ({publishingPlugin.Guid}) published service '{typeof(TInterface).FullName}'.");

         // Raise the event for service publication
         ServicePublished?.Invoke(this, new(publishingPlugin, typeof(TInterface), serviceInstance));
         return true;
      }
   }

   public bool UnpublishService<TInterface>(IPlugin publishingPlugin) where TInterface : class
   {
      ArgumentNullException.ThrowIfNull(publishingPlugin);

      lock (_lock)
      {
         if (_publishedServices.TryGetValue(publishingPlugin.Guid, out var servicesByThisPlugin))
            if (servicesByThisPlugin.Remove(typeof(TInterface)))
            {
               Log($"Plugin '{publishingPlugin.Name}' ({publishingPlugin.Guid}) unpublished service '{typeof(TInterface).FullName}'.");
               // Instance might not be needed for unpublish event
               ServiceUnpublished?.Invoke(this, new(publishingPlugin, typeof(TInterface), null!));
               if (servicesByThisPlugin.Count == 0)
                  _publishedServices.Remove(publishingPlugin.Guid);
               return true;
            }

         Log($"Plugin '{publishingPlugin.Name}' ({publishingPlugin.Guid}) tried to unpublish service '{typeof(TInterface).FullName}', but it was not found.",
             LoggingVerbosity.Warning);
         return false;
      }
   }

   public void UnpublishAllServices(IPlugin publishingPlugin, bool raiseEvent)
   {
      ArgumentNullException.ThrowIfNull(publishingPlugin);

      lock (_lock)
      {
         if (_publishedServices.Remove(publishingPlugin.Guid))
            Log($"Plugin '{publishingPlugin.Name}' ({publishingPlugin.Guid}) unpublished all its services.");
         // Raise the event for unpublishing all services
         if (raiseEvent)
            if (_publishedServices.Count > 0)
               foreach (var serviceType in _publishedServices[publishingPlugin.Guid].Keys)
                  ServiceUnpublished?.Invoke(this, new(publishingPlugin, serviceType, null!));
      }
   }

   public TInterface? GetPublishedService<TInterface>(Guid sourcePluginGuid) where TInterface : class
   {
      lock (_lock)
      {
         if (_publishedServices.TryGetValue(sourcePluginGuid, out var servicesBySourcePlugin))
            if (servicesBySourcePlugin.TryGetValue(typeof(TInterface), out var serviceInstance))
               return serviceInstance as TInterface;

         return null;
      }
   }

   public IEnumerable<(IPlugin publisher, TInterface service)> GetPublishedServices<TInterface>()
      where TInterface : class
   {
      lock (_lock)
         foreach (var pluginServicesPair in _publishedServices)
         {
            var publisher = _infoService.GetPluginByGuid(pluginServicesPair.Key);
            if (publisher != null &&
                pluginServicesPair.Value.TryGetValue(typeof(TInterface), out var serviceInstance) &&
                serviceInstance is TInterface typedService)
               yield return (publisher, typedService);
         }
   }

   public event EventHandler<ServicePublicationEventArgs>? ServicePublished;
   public event EventHandler<ServicePublicationEventArgs>? ServiceUnpublished;

   public void Unload()
   {
   }

   // THe CrossPluginCommunicatorService is only invalid if the host or the info service is null,
   // but since we throw exceptions in the constructor if they are null,
   // we can safely return Ok state here.
   public IService.ServiceState VerifyState() => IService.ServiceState.Ok;
}