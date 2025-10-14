using Arcanum.API.UtilServices;

namespace Arcanum.API.CrossPluginServices;

/// <summary>
/// Provides a mechanism for managing service instances that can be shared across plugins.
/// Allows plugins to publish, unpublish, and retrieve services, as well as to handle service publication events.
/// </summary>
public interface ICrossPluginCommunicator : ISubroutineLogger, IService
{
   /// <summary>
   /// Publishes a service instance provided by the specified plugin.
   /// The service is registered under the given interface type.
   /// A plugin can only publish one instance of a specific service interface type.
   /// </summary>
   /// <typeparam name="TInterface">The interface type under which the service is published.</typeparam>
   /// <param name="publishingPlugin">The plugin instance that is publishing the service.</param>
   /// <param name="serviceInstance">The instance of the service to publish.</param>
   /// <returns>True if the service was published successfully; false if the plugin has already published a service of this type.</returns>
   bool PublishService<TInterface>(IPlugin publishingPlugin, TInterface serviceInstance) where TInterface : class;

   /// <summary>
   /// Unpublishes a specific service type previously published by the given plugin.
   /// </summary>
   /// <typeparam name="TInterface">The interface type of the service to unpublish.</typeparam>
   /// <param name="publishingPlugin">The plugin instance that originally published the service.</param>
   /// <returns>True if the service was found and unpublished successfully; false otherwise.</returns>
   bool UnpublishService<TInterface>(IPlugin publishingPlugin) where TInterface : class;

   /// <summary>
   /// Unpublishes all services previously published by the given plugin.
   /// This is typically called when a plugin is being disabled or unloaded.
   /// </summary>
   /// <param name="publishingPlugin">The plugin instance whose services are to be unpublished.</param>
   /// <param name="raiseEvent"></param>
   void UnpublishAllServices(IPlugin publishingPlugin, bool raiseEvent = true);

   /// <summary>
   /// Retrieves a service instance published by a specific plugin under the given interface type.
   /// </summary>
   /// <typeparam name="TInterface">The interface type of the service to retrieve.</typeparam>
   /// <param name="sourcePluginGuid">The GUID of the plugin that published the service.</param>
   /// <returns>The service instance if found and of the correct type; otherwise, null.</returns>
   TInterface? GetPublishedService<TInterface>(Guid sourcePluginGuid) where TInterface : class;

   /// <summary>
   /// Retrieves all service instances published under a specific interface type by any plugin.
   /// </summary>
   /// <typeparam name="TInterface">The interface type of the services to retrieve.</typeparam>
   /// <returns>An enumerable collection of tuples, each containing the publishing plugin and its service instance.</returns>
   IEnumerable<(IPlugin publisher, TInterface service)> GetPublishedServices<TInterface>() where TInterface : class;

   // Events for service publication/unpublication
   event EventHandler<ServicePublicationEventArgs> ServicePublished;
   event EventHandler<ServicePublicationEventArgs> ServiceUnpublished;
}

public class ServicePublicationEventArgs(IPlugin publishingPlugin, Type serviceType, object serviceInstance)
   : EventArgs
{
   public IPlugin PublishingPlugin { get; } = publishingPlugin;
   public Type ServiceType { get; } = serviceType;
   public object ServiceInstance { get; } = serviceInstance;
}