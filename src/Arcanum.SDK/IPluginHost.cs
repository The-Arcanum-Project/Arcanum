using Arcanum.API.UtilServices;

namespace Arcanum.API;

/// <summary>
/// Defines the contract for a plugin host that provides essential services and facilities
/// for managing plugins in the application.
/// </summary>
public interface IPluginHost
{
   /// <summary>
   /// Retrieves the name associated with the specified GUID.
   /// </summary>
   /// <param name="guid">The GUID for which the name is to be retrieved.</param>
   /// <returns>The name corresponding to the provided GUID, or null if no such name exists.</returns>
   public string GuidToName(Guid guid);

   /// <summary>
   /// Retrieves a service instance of the specified type that is registered with the plugin host.
   /// </summary>
   /// <typeparam name="TIService"></typeparam>
   /// <returns>The instance of the service if found; otherwise, null.</returns>
   public TIService GetService<TIService>() where TIService : class, IService;

   /// <summary>
   /// Registers a service instance of the specified type with the plugin host.
   /// </summary>
   /// <param name="service">The instance of the service to be registered. Cannot be null.</param>
   public void RegisterService<TIService>(TIService service) where TIService : class, IService;

   /// <summary>
   /// Logs a given message to the plugin host's output mechanism.
   /// </summary>
   /// <param name="message">The message to be logged.</param>
   /// <param name="verbosity"></param>
   void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info);

   /// <summary>
   /// Logs a given message to the plugin host's output mechanism.
   /// <c>subRoutinePreFix</c> is used to indicate the internal caller of the log method,
   /// and will be shown in the log output to help identify the source of the log message.
   /// </summary>
   /// <param name="subRoutinePreFix"></param>
   /// <param name="message">The message to be logged.</param>
   /// <param name="verbosity"></param>
   void Log(string subRoutinePreFix, string message, LoggingVerbosity verbosity = LoggingVerbosity.Info);

   /// <summary>
   /// Registers the default service implementations commonly required by plugins which are part of the Arcanum.PluginHost namespace.
   /// </summary>
   void RegisterDefaultServices();

   /// <summary>
   /// Unloads the currently loaded plugins and releases any associated services abd resources managed by the plugin host.
   /// </summary>
   void Unload();

   /// <summary>
   /// Verifies and retrieves the current state of the service.
   /// </summary>
   /// <returns>The current state of the service, represented as a <see cref="IService.ServiceState"/> enumeration value.</returns>
   IService.ServiceState VerifyState();
}

public interface ISubroutineLogger
{
   /// <summary>
   /// Logs a message with a subroutine prefix to indicate the source of the log message.
   /// </summary>
   /// <param name="message">The message to be logged.</param>
   /// <param name="verbosity">The verbosity level of the log message.</param>
   void Log(string message, LoggingVerbosity verbosity = LoggingVerbosity.Info);
}